using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class DocumentAccessLogRepository : GenericRepository<DocumentAccessLog>, IDocumentAccessLogRepository
    {
        public DocumentAccessLogRepository(AppDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DocumentAccessLog>> GetByDocumentIdAsync(Guid documentId)
        {
            return await _context.Set<DocumentAccessLog>()
                .Include(l => l.User)
                .Where(l => l.DocumentId == documentId)
                .OrderByDescending(l => l.AccessedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentAccessLog>> GetByUserIdAsync(string userId)
        {
            return await _context.Set<DocumentAccessLog>()
                .Include(l => l.Document)
                .Where(l => l.AccessedBy == userId)
                .OrderByDescending(l => l.AccessedAt)
                .ToListAsync();
        }
    }
}
