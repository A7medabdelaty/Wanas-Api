using Wanas.Application.DTOs.Verification;

namespace Wanas.Application.Interfaces
{
    public interface IVerificationService
    {
        Task<VerificationStatusDto> UploadVerificationDocumentsAsync(string userId, UploadVerificationDocumentsDto dto);
        Task<VerificationStatusDto> GetVerificationStatusAsync(string userId);
        Task<byte[]> GetDocumentAsync(Guid documentId, string requesterId);
        Task<bool> ReviewVerificationAsync(ReviewVerificationDto dto, string reviewerId);
        Task<List<VerificationStatusDto>> GetPendingVerificationsAsync();
    }
}
