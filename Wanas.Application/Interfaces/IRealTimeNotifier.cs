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
    }
}
