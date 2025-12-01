
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class BookingApprovalService:IBookingApprovalService
    {
        private readonly IUnitOfWork _uow;
        private readonly IChatService _chatService;
        private readonly IRealTimeNotifier _notifier;

        public BookingApprovalService(IUnitOfWork uow, IChatService chatService, IRealTimeNotifier notifier)
        {
            _uow = uow;
            _chatService = chatService;
            _notifier = notifier;
        }

        public async Task<bool> ApprovePaymentAsync(int listingId, string ownerId, string userId)
        {
            // Validate listing ownership
            var listing = await _uow.Listings.GetByIdAsync(listingId);
            if (listing == null || listing.UserId != ownerId)
                return false;

            // Already approved?
            if (await _uow.BookingApprovals.IsApprovedAsync(listingId, userId))
                return false;

            var approval = new BookingApproval
            {
                ListingId = listingId,
                UserId = userId,
                ApprovedAt = DateTime.UtcNow
            };

            await _uow.BookingApprovals.AddAsync(approval);
            await _uow.CommitAsync();

            // Push SignalR to user
            await _notifier.NotifyPaymentApprovedAsync(listingId, userId);

            return true;
        }

        public async Task<bool> ApproveToGroupAsync(int listingId, string ownerId, string userId)
        {
            // Validate
            var listing = await _uow.Listings.GetByIdAsync(listingId);
            if (listing == null || listing.UserId != ownerId)
                return false;

            // Group chat ID
            var groupChatId = listing.GroupChatId;
            if (groupChatId == 0)
                return false;

            var success = await _chatService.AddParticipantAsync(groupChatId, userId);

            if (!success)
                return false;

            return true;
        }
    }
}
