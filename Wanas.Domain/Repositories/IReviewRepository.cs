using Wanas.Domain.Entities;
using Wanas.Domain.Enums;

namespace Wanas.Domain.Repositories
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsForListingAsync(string listingId);
        Task<IEnumerable<Review>> GetReviewsForUserAsync(string userId);
        Task<IEnumerable<Review>> GetReviewsByReviewerAsync(string reviewerId);
        Task<IEnumerable<Review>> GetReviewsByTargetAsync(string targetId, ReviewTarget targetType);
        Task<Review?> GetByIdWithReviewerAsync(int id);
        Task<bool> HasUserReviewedAsync(string reviewerId, string targetId, ReviewTarget targetType); 
        Task<double> GetAverageRatingAsync(string targetId);
        Task<Dictionary<int, double>> GetAverageRatingsForListingIdsAsync(IEnumerable<int> listingIds);
        Task<IEnumerable<Review>> GetLatestReviewsAsync(string targetId, int count);
    }
}
