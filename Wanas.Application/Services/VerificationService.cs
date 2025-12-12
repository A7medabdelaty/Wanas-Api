using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Wanas.Application.DTOs.Verification;
using Wanas.Application.Interfaces;
using Wanas.Application.Settings;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISecureFileService _secureFileService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly VerificationSettings _settings;

        public VerificationService(
            IUnitOfWork unitOfWork,
            ISecureFileService secureFileService,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IOptions<VerificationSettings> settings)
        {
            _unitOfWork = unitOfWork;
            _secureFileService = secureFileService;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _settings = settings.Value;
        }

        public async Task<VerificationStatusDto> UploadVerificationDocumentsAsync(string userId, UploadVerificationDocumentsDto dto)
        {
            // Check if user already has pending verification
            var hasPending = await _unitOfWork.VerificationDocuments.HasPendingVerificationAsync(userId);
            if (hasPending)
                throw new InvalidOperationException("You already have a pending verification request");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (user.IsVerified)
                throw new InvalidOperationException("User is already verified");

            // Validate all files
            if (!await _secureFileService.ValidateFileAsync(dto.NationalIdFront) ||
          !await _secureFileService.ValidateFileAsync(dto.NationalIdBack) ||
          !await _secureFileService.ValidateFileAsync(dto.SelfieWithId))
            {
                throw new InvalidOperationException("One or more files are invalid");
            }

            var documents = new List<VerificationDocument>();

            try
            {
                // Upload National ID Front
                var (frontPath, frontHash) = await _secureFileService.SaveVerificationDocumentAsync(
                       dto.NationalIdFront, userId, "NationalIdFront");
                documents.Add(new VerificationDocument
                {
                    UserId = userId,
                    DocumentType = DocumentType.NationalIdFront,
                    EncryptedFilePath = frontPath,
                    FileHash = frontHash
                });

                // Upload National ID Back
                var (backPath, backHash) = await _secureFileService.SaveVerificationDocumentAsync(dto.NationalIdBack, userId, "NationalIdBack");
                documents.Add(new VerificationDocument
                {
                    UserId = userId,
                    DocumentType = DocumentType.NationalIdBack,
                    EncryptedFilePath = backPath,
                    FileHash = backHash
                });

                // Upload Selfie with ID
                var (selfiePath, selfieHash) = await _secureFileService.SaveVerificationDocumentAsync(
                           dto.SelfieWithId, userId, "SelfieWithId");
                documents.Add(new VerificationDocument
                {
                    UserId = userId,
                    DocumentType = DocumentType.SelfieWithId,
                    EncryptedFilePath = selfiePath,
                    FileHash = selfieHash
                });

                // Save all documents
                foreach (var doc in documents)
                {
                    await _unitOfWork.VerificationDocuments.AddAsync(doc);
                }

                // Update user verification status
                user.VerificationSubmittedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Log access
                await LogDocumentAccessAsync(documents, userId, "Upload");

                await _unitOfWork.CommitAsync();

                return await GetVerificationStatusAsync(userId);
            }
            catch
            {
                // Rollback file uploads on error
                foreach (var doc in documents)
                {
                    await _secureFileService.DeleteVerificationDocumentAsync(doc.EncryptedFilePath);
                }
                throw;
            }
        }

        public async Task<VerificationStatusDto> GetVerificationStatusAsync(string userId)
        {
            var documents = await _unitOfWork.VerificationDocuments.GetByUserIdAsync(userId);
            var user = await _userManager.FindByIdAsync(userId);

            var documentDtos = documents.Select(d => new VerificationDocumentDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                Status = d.Status,
                UploadedAt = d.UploadedAt,
                ReviewedAt = d.ReviewedAt,
                RejectionReason = d.RejectionReason
            }).ToList();

            var latestDoc = documents.OrderByDescending(d => d.UploadedAt).FirstOrDefault();

            return new VerificationStatusDto
            {
                HasSubmitted = documents.Any(),
                IsVerified = user?.IsVerified ?? false,
                Status = latestDoc?.Status,
                SubmittedAt = user?.VerificationSubmittedAt,
                ReviewedAt = latestDoc?.ReviewedAt,
                Documents = documentDtos
            };
        }

        public async Task<byte[]> GetDocumentAsync(Guid documentId, string requesterId)
        {
            var document = await _unitOfWork.VerificationDocuments.GetByIdAsync(documentId);
            if (document == null)
                throw new InvalidOperationException("Document not found");

            // Check authorization - only user or admin can access
            var requester = await _userManager.FindByIdAsync(requesterId);
            var isAdmin = await _userManager.IsInRoleAsync(requester!, "Admin");

            if (document.UserId != requesterId && !isAdmin)
                throw new UnauthorizedAccessException("You don't have permission to access this document");

            // Log access
            await LogDocumentAccessAsync(new List<VerificationDocument> { document }, requesterId, "View");

            return await _secureFileService.GetVerificationDocumentAsync(document.EncryptedFilePath);
        }

        public async Task<bool> ReviewVerificationAsync(ReviewVerificationDto dto, string reviewerId)
        {
            var documents = await _unitOfWork.VerificationDocuments.GetByUserIdAsync(dto.UserId);
            var pendingDocs = documents.Where(d => d.Status == VerificationStatus.Pending).ToList();

            if (!pendingDocs.Any())
                throw new InvalidOperationException("No pending verification documents found");

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var doc in pendingDocs)
                {
                    doc.Status = dto.Status;
                    doc.ReviewedAt = DateTime.UtcNow;
                    doc.ReviewedBy = reviewerId;
                    doc.RejectionReason = dto.RejectionReason;

                    // Schedule deletion based on settings
                    if (_settings.EnableAutoDelete)
                    {
                        doc.ScheduledDeletionDate = DateTime.UtcNow.AddDays(_settings.DocumentRetentionDays);
                    }

                    _unitOfWork.VerificationDocuments.Update(doc);
                }

                // Update user verification status
                if (dto.Status == VerificationStatus.Approved)
                {
                    user.IsVerified = true;
                    user.VerificationApprovedAt = DateTime.UtcNow;
                }
                else if (dto.Status == VerificationStatus.Rejected)
                {
                    user.IsVerified = false;
                    user.VerificationSubmittedAt = null;
                }

                await _userManager.UpdateAsync(user);

                // Log access
                await LogDocumentAccessAsync(pendingDocs, reviewerId, "Review");

                // Create audit log
                await _unitOfWork.AuditLogs.AddAsync(new AuditLog
                {
                    Action = dto.Status == VerificationStatus.Approved ? "VerifyUser" : "RejectVerification",
                    AdminId = reviewerId,
                    TargetUserId = dto.UserId,
                    Details = dto.RejectionReason ?? $"Verification {dto.Status}"
                });

                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<VerificationStatusDto>> GetPendingVerificationsAsync()
        {
            var pendingDocs = await _unitOfWork.VerificationDocuments.GetPendingDocumentsAsync();

            var groupedByUser = pendingDocs.GroupBy(d => d.UserId);

            var result = new List<VerificationStatusDto>();

            foreach (var group in groupedByUser)
            {
                var user = await _userManager.FindByIdAsync(group.Key);
                var documents = group.Select(d => new VerificationDocumentDto
                {
                    Id = d.Id,
                    DocumentType = d.DocumentType,
                    Status = d.Status,
                    UploadedAt = d.UploadedAt,
                    ReviewedAt = d.ReviewedAt,
                    RejectionReason = d.RejectionReason
                }).ToList();

                result.Add(new VerificationStatusDto
                {
                    HasSubmitted = true,
                    Status = VerificationStatus.Pending,
                    SubmittedAt = user?.VerificationSubmittedAt,
                    Documents = documents
                });
            }

            return result;
        }

        private async Task LogDocumentAccessAsync(
                 List<VerificationDocument> documents, string userId, string action)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            foreach (var doc in documents)
            {
                var log = new DocumentAccessLog
                {
                    DocumentId = doc.Id,
                    AccessedBy = userId,
                    Action = action,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                await _unitOfWork.DocumentAccessLogs.AddAsync(log);
            }
        }
    }
}
