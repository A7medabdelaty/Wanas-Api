namespace Wanas.Application.DTOs.Approval
{
    public class GroupApprovalRequest
    {
        public int ListingId { get; set; }
        public string OwnerId { get; set; }
        public string UserId { get; set; }
    }
}
