using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wanas.Application.DTOs.Review;
using Wanas.Application.Interfaces;
using Wanas.Domain.Enums;

namespace Wanas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService revServ;
        public ReviewsController(IReviewService revServ)
        {
            this.revServ = revServ;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst("sub")?.Value; // JWT user ID
            if (userId == null) return Unauthorized();

            var result = await revServ.CreateReviewAsync(dto, userId);
            return Ok(result);
        }

        [HttpPut("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst("sub")?.Value;
            if (userId == null) return Unauthorized();

            var result = await revServ.UpdateReviewAsync(reviewId, dto, userId);
            return Ok(result);
        }

        [HttpDelete("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null) return Unauthorized();

            bool success = await revServ.DeleteReviewAsync(reviewId, userId);
            if (!success) return NotFound();

            return NoContent();
        }

        [HttpGet("{reviewId}")]
        public async Task<IActionResult> GetReviewById(int reviewId)
        {
            var review = await revServ.GetReviewByIdAsync(reviewId);
            if (review == null) return NotFound();

            return Ok(review);
        }

        [HttpGet("listing/{listingId}")]
        public async Task<IActionResult> GetReviewsForListing(string listingId)
        {
            var reviews = await revServ.GetReviewsForListingAsync(listingId);
            return Ok(reviews);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetReviewsForUser(string userId)
        {
            var reviews = await revServ.GetReviewsForUserAsync(userId);
            return Ok(reviews);
        }

        [HttpGet("target")]
        public async Task<IActionResult> GetReviewsByTarget([FromQuery] string targetId, [FromQuery] ReviewTarget targetType)
        {
            var reviews = await revServ.GetReviewsByTargetAsync(targetId, targetType);
            return Ok(reviews);
        }

        [HttpGet("reviewer/{reviewerId}")]
        public async Task<IActionResult> GetReviewsByReviewer(string reviewerId)
        {
            var reviews = await revServ.GetReviewsByReviewerAsync(reviewerId);
            return Ok(reviews);
        }

        [HttpGet("has-reviewed")]
        [Authorize]
        public async Task<IActionResult> HasUserReviewed([FromQuery] string targetId, [FromQuery] ReviewTarget targetType)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null) return Unauthorized();

            var result = await revServ.HasUserReviewedAsync(userId, targetId, targetType);
            return Ok(new { hasReviewed = result });
        }

        [HttpGet("average-rating/{targetId}")]
        public async Task<IActionResult> GetAverageRating(string targetId)
        {
            double rating = await revServ.GetAverageRatingAsync(targetId);
            return Ok(rating);
        }

        [HttpGet("latest/{targetId}")]
        public async Task<IActionResult> GetLatestReviews(string targetId, [FromQuery] int count = 5)
        {
            var reviews = await revServ.GetLatestReviewsAsync(targetId, count);
            return Ok(reviews);
        }
    }
}
