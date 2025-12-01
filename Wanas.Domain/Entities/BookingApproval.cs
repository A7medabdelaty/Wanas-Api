namespace Wanas.Domain.Entities
{
    public class BookingApproval
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public Listing Listing { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
    }
}
