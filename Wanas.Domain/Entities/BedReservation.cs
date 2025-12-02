namespace Wanas.Domain.Entities
{
    public class BedReservation
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
        public bool IsConfirmed { get; set; } = false;
        public DateTime? ConfirmedAt { get; set; }
        public bool IsCancelled { get; set; } = false;
        public DateTime? CancelledAt { get; set; }

        public virtual ICollection<BedReservationItem> Items { get; set; } = new List<BedReservationItem>();
    }
}
