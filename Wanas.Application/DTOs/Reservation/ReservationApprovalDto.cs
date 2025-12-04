using Wanas.Domain.Entities;

namespace Wanas.Application.DTOs.Reservation
{
    public class ReservationApprovalDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public string OwnerId { get; set; }
        public ApprovalStatus Status { get; set; }
    }
}
