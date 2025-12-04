using Wanas.Application.DTOs.Listing;
using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.Reservation
{
    public class ReservationListItemDto
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public string ListingTitle { get; set; }
        public string City { get; set; }
        public string CoverPhotoUrl { get; set; }

        public DateTime StartDate { get; set; }
        public int DurationInDays { get; set; }
        public decimal TotalPrice { get; set; }

        public PaymentStatus PaymentStatus { get; set; }
        public decimal? DepositAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        public List<BedDto> Beds { get; set; } = new();
    }
}
