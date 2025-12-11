using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
    public interface IDocumentAccessLogRepository : IGenericRepository<DocumentAccessLog>
    {
        Task<IEnumerable<DocumentAccessLog>> GetByDocumentIdAsync(Guid documentId);
        Task<IEnumerable<DocumentAccessLog>> GetByUserIdAsync(string userId);
    }
}
