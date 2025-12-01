using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    public class BookingApproval
    {
        public int Id { get; set; }

        // Main keys
        public int ListingId { get; set; }
        public string UserId { get; set; }        // The requester who wants to book/pay
        public string OwnerId { get; set; }       // Listing owner
        public int ChatId { get; set; }           // 1-to-1 chat where the approval happened

        public ApprovalType Type { get; set; }    // Payment or Group
        public DateTime ApprovedAt { get; set; }

        // Navigation Properties
        public Listing Listing { get; set; }
        public ApplicationUser User { get; set; }
        public ApplicationUser Owner { get; set; }
        public Chat Chat { get; set; }
    }

}
