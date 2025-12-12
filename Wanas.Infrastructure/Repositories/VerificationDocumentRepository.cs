using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class VerificationDocumentRepository : GenericRepository<VerificationDocument>, IVerificationDocumentRepository
    {
        public VerificationDocumentRepository(AppDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<VerificationDocument>> GetByUserIdAsync(string userId)
        {
            return await _context.Set<VerificationDocument>()
                .Where(d => d.UserId == userId && !d.IsDeleted)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<VerificationDocument?> GetByUserIdAndTypeAsync(string userId, DocumentType documentType)
        {
            return await _context.Set<VerificationDocument>()
                .FirstOrDefaultAsync(d => d.UserId == userId && d.DocumentType == documentType && !d.IsDeleted);
        }

        public async Task<IEnumerable<VerificationDocument>> GetPendingDocumentsAsync()
        {
            return await _context.Set<VerificationDocument>()
               .Include(d => d.User)
               .Where(d => d.Status == VerificationStatus.Pending && !d.IsDeleted)
               .OrderBy(d => d.UploadedAt)
               .ToListAsync();
        }

        public async Task<IEnumerable<VerificationDocument>> GetDocumentsForDeletionAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Set<VerificationDocument>()
                .Where(d => !d.IsDeleted &&d.ScheduledDeletionDate.HasValue &&d.ScheduledDeletionDate.Value <= now)
                .ToListAsync();
        }

        public async Task<bool> HasPendingVerificationAsync(string userId)
        {
            return await _context.Set<VerificationDocument>()
                .AnyAsync(d => d.UserId == userId &&d.Status == VerificationStatus.Pending &&!d.IsDeleted);
        }
    }
}
