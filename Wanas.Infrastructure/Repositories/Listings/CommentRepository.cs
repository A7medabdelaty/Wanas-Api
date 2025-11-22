using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
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
                .Where(c => c.ListingId == listingId && c.DeletedAt == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsByUserAsync(string userId)
        {
            return await _context.Comments
                .Where(c => c.AuthorId == userId && c.DeletedAt == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetRepliesAsync(int parentCommentId)
        {
            return await _context.Comments
                .Where(c => c.ParentCommentId == parentCommentId && c.DeletedAt == null)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}
