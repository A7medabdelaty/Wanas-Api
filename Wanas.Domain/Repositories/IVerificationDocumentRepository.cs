using Wanas.Domain.Entities;
using Wanas.Domain.Enums;

namespace Wanas.Domain.Repositories
{
    public interface IVerificationDocumentRepository : IGenericRepository<VerificationDocument>
    {
        Task<IEnumerable<VerificationDocument>> GetByUserIdAsync(string userId);
        Task<VerificationDocument?> GetByUserIdAndTypeAsync(string userId, DocumentType documentType);
        Task<IEnumerable<VerificationDocument>> GetPendingDocumentsAsync();
        Task<IEnumerable<VerificationDocument>> GetDocumentsForDeletionAsync();
        Task<bool> HasPendingVerificationAsync(string userId);
    }
}
