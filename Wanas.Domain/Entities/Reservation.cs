using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ListingId { get; set; }

        public ReservationStatus Status { get; set; }
        public decimal DepositAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime PaidAt { get; set; }

        public List<Bed> ReservedBeds { get; set; } = new();

        public string? OwnerId { get; set; }
    }
}
