using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    public class Listing
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public int MonthlyPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ApartmentListing ApartmentListing { get; set; }
        public HashSet<ListingPhoto> ListingPhotos { get; set; } = new();
        public HashSet<Payment> Payments { get; set; } = new();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Match> Matches { get; set; } = new List<Match>();

        public ListingModerationStatus ModerationStatus { get; set; } = ListingModerationStatus.Pending;
        public string? ModeratedByAdminId { get; set; }
        public DateTime? ModeratedAt { get; set; }
        public string? ModerationNote { get; set; }
        public bool IsFlagged { get; set; } = false;
        public string? FlagReason { get; set; }

        // GROUP CHAT (1:1)
        public int? GroupChatId { get; set; }
        public Chat? GroupChat { get; set; }

        // PRIVATE CHATS (1:M)
        public ICollection<Chat> Chats { get; set; } = new List<Chat>();

        public ICollection<BookingApproval> BookingApprovals { get; set; } = new List<BookingApproval>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }

}
