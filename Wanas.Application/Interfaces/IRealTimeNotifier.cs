using Wanas.Application.DTOs.Chat;
using Wanas.Application.DTOs.Message;

namespace Wanas.Application.Interfaces
{
    public interface IRealTimeNotifier
    {
        Task NotifyChatCreatedAsync(ChatDto chat);
        Task NotifyChatUpdatedAsync(ChatDto chat);
        Task NotifyChatDeletedAsync(int chatId);
        Task NotifyParticipantAddedAsync(int chatId, string userId);
        Task NotifyParticipantRemovedAsync(int chatId, string userId);
        Task NotifyMessageReceivedAsync(MessageDto message);
        Task NotifyMessageDeletedAsync(int chatId, int messageId);
        Task NotifyMessageReadAsync(int chatId, int messageId, string userId);
        Task NotifyPaymentApprovedAsync(int listingId, string userId);
        Task NotifyGroupApprovedAsync(int chatId, string userId);
        Task NotifyOwnerAsync(string ownerId, string message);
        Task NotifyUserAsync(string userId, string message);
        Task NotifyUserTypingAsync(int chatId, string userId, string userName);
        Task NotifyUserStoppedTypingAsync(int chatId, string userId);
        // User presence
        Task NotifyUserStatusChangedAsync(string userId, bool isOnline);
        // Listing notifications
        Task NotifyListingUpdatedAsync(int listingId, string ownerId);
        // Reservation notifications
        Task NotifyReservationCreatedAsync(int reservationId, string renterId, string ownerId);
        Task NotifyReservationUpdatedAsync(int reservationId, string userId);
        Task NotifyReservationCancelledAsync(int reservationId, string userId);
    }
}
