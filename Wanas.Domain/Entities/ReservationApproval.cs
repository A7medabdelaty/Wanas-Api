namespace Wanas.Domain.Entities
{
    public class ReservationApproval
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public string OwnerId { get; set; } = default!;
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Reservation Reservation { get; set; } = default!;
    }
    public enum ApprovalStatus { Pending, Approved, Rejected }

}
