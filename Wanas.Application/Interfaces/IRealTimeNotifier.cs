using Wanas.Application.DTOs.Chat;
using Wanas.Application.DTOs.Message;

namespace Wanas.Application.Interfaces
{
    public interface IRealTimeNotifier
    {
        Task NotifyChatCreatedAsync(ChatDto chat);
        Task NotifyParticipantAddedAsync(int chatId, string userId);
        Task NotifyParticipantRemovedAsync(int chatId, string userId);
        Task NotifyMessageReceivedAsync(MessageDto message);
        Task NotifyMessageDeletedAsync(int chatId, int messageId);
        Task NotifyMessageReadAsync(int chatId, int messageId, string userId);
    }
}
