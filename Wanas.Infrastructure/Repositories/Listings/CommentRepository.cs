using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories.Listings;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories.Listings
{
    public class CommentRepository : GenericRepository<Comment>, ICommentRepository
    {
        public CommentRepository(AppDBContext context) : base(context)
        {

        }

        public async Task<IEnumerable<Comment>> GetCommentsByListingAsync(int listingId)
        {
            return await _context.Comments
                .Where(c => c.ListingId == listingId)
                .Include(c=>c.Author)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsByUserAsync(string userId)
        {
            return await _context.Comments
                .Where(c => c.AuthorId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> GetCommentWithAuthorAndRepliesAsync(int commentId)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.Author)
                .FirstOrDefaultAsync(c => c.Id == commentId);
        }
    }
}
