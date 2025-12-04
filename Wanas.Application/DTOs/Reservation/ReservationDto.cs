using Wanas.Application.DTOs.Listing;
using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.Reservation
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public string UserId { get; set; }

        public PaymentStatus PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal RemainingAmount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string ListingTitle { get; set; }
        public string OwnerId { get; set; }

        public List<BedDto> Beds { get; set; }
    }
}
