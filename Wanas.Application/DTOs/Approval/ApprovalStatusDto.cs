namespace Wanas.Application.DTOs.Approval
{
    public class ApprovalStatusDto
    {
        public int ListingId { get; set; }
        public string UserId { get; set; }

        public bool CanChat { get; set; }
        public bool CanJoinGroup { get; set; }
        public bool CanPay { get; set; }

        public bool IsGroupApproved { get; set; }
        public bool IsPaymentApproved { get; set; }
    }
}
