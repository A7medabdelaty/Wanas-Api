
using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(AppDBContext context) : base(context)
        {

        }

        public async Task<double> GetAverageRatingAsync(string targetId)
        {
            var ratings = await _context.Reviews
                .Where(r => r.TargetId == targetId)
                .Select(r => (double)r.Rating)
                .ToListAsync();

            if (ratings.Count == 0) return 0;
            return ratings.Average();
        }

        public async Task<IEnumerable<Review>> GetReviewsByReviewerAsync(string reviewerId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ReviewerId == reviewerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews;
        }

        public async Task<IEnumerable<Review>> GetReviewsForListingAsync(string listingId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.TargetId == listingId)
                .Include(r=>r.Reviewer)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews;
        }

        public async Task<IEnumerable<Review>> GetReviewsForUserAsync(string userId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.TargetId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews;
        }

        public async Task<bool> HasUserReviewedAsync(string reviewerId, string targetId, ReviewTarget targetType)
        {
            return await _context.Reviews
                .AnyAsync(r =>
                    r.ReviewerId == reviewerId &&
                    r.TargetId == targetId &&
                    r.TargetType == targetType);
        }

        public async Task<IEnumerable<Review>> GetLatestReviewsAsync(string targetId, int count)
        {
            return await _context.Reviews
                .Where(r => r.TargetId == targetId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByTargetAsync(string targetId, ReviewTarget targetType)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.TargetId == targetId && r.TargetType == targetType)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews;
        }

        public async Task<Review?> GetByIdWithReviewerAsync(int id)
        {
            var review = await _context.Reviews
               .Include(r => r.Reviewer)
               .FirstOrDefaultAsync(r => r.ReviewId == id);

            return review;
        }
    }
}
