
using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.Review
{
    public class CreateReviewDto
    {
        public ReviewTarget TargetType { get; set; }
        public string TargetId { get; set; } = string.Empty; // userId or listingId
        public int Rating { get; set; } // 1..5
        public string Comment { get; set; } = string.Empty;
    }
}
