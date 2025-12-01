namespace Wanas.Application.Interfaces
{
    public interface IBookingApprovalService
    {
        Task<bool> ApprovePaymentAsync(int listingId, string ownerId, string userId);
        Task<bool> ApproveToGroupAsync(int listingId, string ownerId, string userId);
    }
}
