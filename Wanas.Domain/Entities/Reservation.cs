using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    public class Reservation
    {
        public int Id { get; set; }

        public int ListingId { get; set; }
        public Listing Listing { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal RemainingAmount { get; set; }

        public PaymentStatus PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BedReservation> Beds { get; set; } = new List<BedReservation>();
    }
}
