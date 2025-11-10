using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    public class Review
    {
        public int ReviewId { get; set; }
        public ReviewTarget TargetType { get; set; }
        public string TargetId { get; set; } // store userId or listingId as string - simpler
        public int Rating { get; set; } // 1..5
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string ReviewerId { get; set; }
        public ApplicationUser Reviewer { get; set; }
    }
}
