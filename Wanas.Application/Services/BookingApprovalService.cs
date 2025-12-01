
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class BookingApprovalService : IBookingApprovalService
    {
        private readonly IUnitOfWork _uow;
        private readonly IChatService _chatService;
        private readonly IRealTimeNotifier _notifier;

        public BookingApprovalService(
            IUnitOfWork uow,
            IChatService chatService,
            IRealTimeNotifier notifier)
        {
            _uow = uow;
            _chatService = chatService;
            _notifier = notifier;
        }

        public async Task<bool> ApproveToGroupAsync(int listingId, string ownerId, string userId)
        {
            var listing = await _uow.Listings.GetByIdAsync(listingId);
            if (listing == null)
                return false;

            // Find or create PRIVATE CHAT between owner & user
            var privateChat = await _chatService.GetPrivateChatForListingAsync(ownerId, userId, listingId);
            if (privateChat == null)
                return false;

            // GROUP approval already exists?
            if (await _uow.BookingApprovals
                    .ExistsAsync(listingId, userId, ApprovalType.Group))
                return false;

            // Insert approval
            await _uow.BookingApprovals.AddAsync(new BookingApproval
            {
                ListingId = listingId,
                UserId = userId,
                Type = ApprovalType.Group,
                ApprovedAt = DateTime.UtcNow
            });

            await _uow.CommitAsync();

            // Notify group chat
            await _notifier.NotifyGroupApprovedAsync(listing.GroupChatId, userId);

            return true;
        }


        public async Task<bool> ApprovePaymentAsync(int listingId, string ownerId, string userId)
        {
            var listing = await _uow.Listings.GetByIdAsync(listingId);
            if (listing == null)
                return false;

            // Ensure PRIVATE CHAT exists
            var privateChat = await _chatService.GetPrivateChatForListingAsync(ownerId, userId, listingId);
            if (privateChat == null)
                return false;

            // Payment approval already exists?
            if (await _uow.BookingApprovals
                    .ExistsAsync(listingId, userId, ApprovalType.Payment))
                return false;

            // Insert approval
            await _uow.BookingApprovals.AddAsync(new BookingApproval
            {
                ListingId = listingId,
                UserId = userId,
                Type = ApprovalType.Payment,
                ApprovedAt = DateTime.UtcNow
            });

            await _uow.CommitAsync();

            // Notify user
            await _notifier.NotifyPaymentApprovedAsync(listingId, userId);

            return true;
        }
    }
}
