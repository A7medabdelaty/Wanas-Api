using AutoMapper;
using Wanas.Application.DTOs.Review;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unit;

        public ReviewService(IMapper mapper, IUnitOfWork unit)
        {
            this.mapper = mapper;
            this.unit = unit;
        }
        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto, string reviewerId)
        {
            //// Prevent duplicate reviews
            //var hasReviewed = await unit.Reviews.HasUserReviewedAsync(reviewerId, dto.TargetId, dto.TargetType);
            //if (hasReviewed)
            //    throw new InvalidOperationException("You have already reviewed this target.");

            var review = mapper.Map<Review>(dto);
            review.ReviewerId = reviewerId;
            review.CreatedAt = DateTime.UtcNow;

            await unit.Reviews.AddAsync(review);
            await unit.CommitAsync();

            // Load Reviewer navigation property
            review = await unit.Reviews.GetByIdWithReviewerAsync(review.ReviewId);

            return mapper.Map<ReviewDto>(review);
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, string requesterId)
        {
            var review = await unit.Reviews.GetByIdAsync(reviewId);

            if (review == null)
                return false;

            if (review.ReviewerId != requesterId)
                throw new UnauthorizedAccessException("You can only delete your own reviews.");

            unit.Reviews.Remove(review);
            await unit.CommitAsync();

            return true;
        }

        public Task<double> GetAverageRatingAsync(string targetId)
        {
            return unit.Reviews.GetAverageRatingAsync(targetId);
        }

        public async Task<IEnumerable<ReviewDto>> GetLatestReviewsAsync(string targetId, int count)
        {
            var reviews = await unit.Reviews.GetLatestReviewsAsync(targetId, count);
            return mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int id)
        {
            var review = await unit.Reviews.GetByIdWithReviewerAsync(id);
            return mapper.Map<ReviewDto?>(review);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsForListingAsync(string listingId)
        {
            var reviews = await unit.Reviews.GetReviewsForListingAsync(listingId);
            return mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByTargetAsync(string targetId, ReviewTarget targetType)
        {
            var reviews = await unit.Reviews.GetReviewsByTargetAsync(targetId, targetType);
            return mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsForUserAsync(string userId)
        {
            var reviews = await unit.Reviews.GetReviewsForUserAsync(userId);
            return mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public Task<bool> HasUserReviewedAsync(string reviewerId, string targetId, ReviewTarget targetType)
        {
            return unit.Reviews.HasUserReviewedAsync(reviewerId, targetId, targetType);
        }

        public async Task<ReviewDto> UpdateReviewAsync(int reviewId, UpdateReviewDto dto, string requesterId)
        {
            var review = await unit.Reviews.GetByIdWithReviewerAsync(reviewId);

            if (review == null)
                throw new KeyNotFoundException("Review not found.");

            if (review.ReviewerId != requesterId)
                throw new UnauthorizedAccessException("You can only edit your own review.");

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;

            unit.Reviews.Update(review);
            await unit.CommitAsync();

            return mapper.Map<ReviewDto>(review);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByReviewerAsync(string reviewerId)
        {
            var reviews = await unit.Reviews.GetReviewsByReviewerAsync(reviewerId);
            return mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }
    }
}
