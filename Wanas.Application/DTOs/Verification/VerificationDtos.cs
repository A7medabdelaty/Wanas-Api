using Microsoft.AspNetCore.Http;
using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.Verification
{
    public class UploadVerificationDocumentsDto
    {
        public IFormFile NationalIdFront { get; set; } = default!;
        public IFormFile NationalIdBack { get; set; } = default!;
        public IFormFile SelfieWithId { get; set; } = default!;
    }

    public class VerificationDocumentDto
    {
        public Guid Id { get; set; }
        public DocumentType DocumentType { get; set; }
        public VerificationStatus Status { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class VerificationStatusDto
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public bool HasSubmitted { get; set; }
        public bool IsVerified { get; set; }
        public VerificationStatus? Status { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public List<VerificationDocumentDto> Documents { get; set; } = new();
    }

    public class ReviewVerificationDto
    {
        public string UserId { get; set; } = default!;
        public VerificationStatus Status { get; set; } // Approved or Rejected
        public string? RejectionReason { get; set; }
    }
}
