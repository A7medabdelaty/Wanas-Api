using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.Listing
{
    public class ListingModerationDto
    {
        public int ListingId { get; set; }
        public ListingModerationStatus ModerationStatus { get; set; }
        public string? ModerationNote { get; set; }
        public bool? IsFlagged { get; set; }
        public string? FlagReason { get; set; }
        public DateTime? ModeratedAt { get; set; }
        public string? ModeratedByAdminId { get; set; }
    }
}