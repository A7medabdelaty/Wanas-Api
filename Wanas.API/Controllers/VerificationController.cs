using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wanas.Application.DTOs.Verification;
using Wanas.Application.Interfaces;

namespace Wanas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationService _verificationService;
        private readonly ILogger<VerificationController> _logger;

        public VerificationController(
            IVerificationService verificationService,
            ILogger<VerificationController> logger)
        {
            _verificationService = verificationService;
            _logger = logger;
        }


        // Upload verification documents (National ID front, back, and selfie with ID)
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadVerificationDocuments([FromForm] UploadVerificationDocumentsDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                _logger.LogInformation("User {UserId} uploading verification documents", userId);

                var result = await _verificationService.UploadVerificationDocumentsAsync(userId, dto);

                _logger.LogInformation("User {UserId} successfully uploaded verification documents", userId);
                return Ok(new
                {
                    message = "Verification documents uploaded successfully. Your submission is under review.",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error for user {UserId}", userId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading verification documents for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while uploading documents" });
            }
        }

        // Get verification status for current user
        [HttpGet("status")]
        public async Task<IActionResult> GetVerificationStatus()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _verificationService.GetVerificationStatusAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting verification status for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        // Get a specific verification document (Admin or document owner only)
        [HttpGet("document/{documentId}")]
        public async Task<IActionResult> GetDocument(Guid documentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var fileBytes = await _verificationService.GetDocumentAsync(documentId, userId);

                // Determine content type based on file signature
                var contentType = DetermineContentType(fileBytes);

                return File(fileBytes, contentType);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt by user {UserId} for document {DocumentId}",
                userId, documentId);
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document {DocumentId}", documentId);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        // Review verification documents (Admin only)
        [HttpPost("review")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReviewVerification([FromBody] ReviewVerificationDto dto)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            try
            {
                _logger.LogInformation("Admin {AdminId} reviewing verification for user {UserId}",
                 adminId, dto.UserId);

                var result = await _verificationService.ReviewVerificationAsync(dto, adminId);

                _logger.LogInformation("Admin {AdminId} completed review for user {UserId} with status {Status}",
                    adminId, dto.UserId, dto.Status);

                return Ok(new { message = "Verification review completed successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error during review", adminId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing verification for user {UserId}", dto.UserId);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        // Get all pending verifications (Admin only)
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingVerifications()
        {
            try
            {
                var result = await _verificationService.GetPendingVerificationsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending verifications");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        private string DetermineContentType(byte[] fileBytes)
        {
            // Check file signature (magic numbers)
            if (fileBytes.Length >= 2)
            {
                // JPEG
                if (fileBytes[0] == 0xFF && fileBytes[1] == 0xD8)
                    return "image/jpeg";

                // PNG
                if (fileBytes[0] == 0x89 && fileBytes[1] == 0x50)
                    return "image/png";

                // PDF
                if (fileBytes.Length >= 4 && fileBytes[0] == 0x25 && fileBytes[1] == 0x50 &&
                  fileBytes[2] == 0x44 && fileBytes[3] == 0x46)
                    return "application/pdf";
            }

            // Default
            return "application/octet-stream";
        }
    }
}
