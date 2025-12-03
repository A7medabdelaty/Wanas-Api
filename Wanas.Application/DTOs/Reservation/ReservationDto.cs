using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.Reservation
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public int ListingId { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal DepositAmount { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public List<int> BedIds { get; set; }
    }
}
