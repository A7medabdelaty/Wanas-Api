using Wanas.Application.DTOs.Review;
using Wanas.Domain.Enums;

namespace Wanas.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto, string reviewerId);
        Task<bool> DeleteReviewAsync(int reviewId, string requesterId);
        Task<ReviewDto> UpdateReviewAsync(int reviewId, UpdateReviewDto dto, string requesterId);
        Task<IEnumerable<ReviewDto>> GetReviewsForListingAsync(string listingId);
        Task<IEnumerable<ReviewDto>> GetReviewsForUserAsync(string userId);
        Task<IEnumerable<ReviewDto>> GetReviewsByReviewerAsync(string reviewerId);
        Task<IEnumerable<ReviewDto>> GetReviewsByTargetAsync(string targetId, ReviewTarget targetType);
        Task<ReviewDto?> GetReviewByIdAsync(int id);
        Task<bool> HasUserReviewedAsync(string reviewerId, string targetId, ReviewTarget targetType);
        Task<double> GetAverageRatingAsync(string targetId);
        Task<IEnumerable<ReviewDto>> GetLatestReviewsAsync(string targetId, int count);
    }
}
