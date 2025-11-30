using Microsoft.AspNetCore.SignalR;
using Wanas.API.Hubs;
using Wanas.Application.DTOs.Chat;
using Wanas.Application.DTOs.Message;
using Wanas.Application.Interfaces;

namespace Wanas.API.RealTime
{
    public class RealTimeNotifier : IRealTimeNotifier
    {
        private readonly IHubContext<ChatHub> _hub;

        public RealTimeNotifier(IHubContext<ChatHub> hub)
        {
            _hub = hub;
        }

        // ----------------------------------------------------
        // CHAT CREATED
        // ----------------------------------------------------
        public async Task NotifyChatCreatedAsync(ChatDto chat)
        {
            foreach (var participant in chat.Participants)
            {
                await _hub.Clients.User(participant.UserId)
                    .SendAsync("ChatCreated", chat);
            }
        }

        // ----------------------------------------------------
        // CHAT UPDATED
        // ----------------------------------------------------
        public async Task NotifyChatUpdatedAsync(ChatDto chat)
        {
            await _hub.Clients.Group($"chat_{chat.Id}")
                .SendAsync("ChatUpdated", chat);
        }

        // ----------------------------------------------------
        // CHAT DELETED
        // ----------------------------------------------------
        public async Task NotifyChatDeletedAsync(int chatId)
        {
            await _hub.Clients.Group($"chat_{chatId}")
                .SendAsync("ChatDeleted", chatId);
        }

        // ----------------------------------------------------
        // PARTICIPANT ADDED
        // ----------------------------------------------------
        public async Task NotifyParticipantAddedAsync(int chatId, string userId)
        {
            await _hub.Clients.Group($"chat_{chatId}")
                .SendAsync("ParticipantAdded", chatId, userId);
        }

        // ----------------------------------------------------
        // PARTICIPANT REMOVED
        // ----------------------------------------------------
        public async Task NotifyParticipantRemovedAsync(int chatId, string userId)
        {
            await _hub.Clients.Group($"chat_{chatId}")
                .SendAsync("ParticipantRemoved", chatId, userId);
        }

        // ----------------------------------------------------
        // MESSAGE RECEIVED
        // ----------------------------------------------------
        public async Task NotifyMessageReceivedAsync(MessageDto message)
        {
            await _hub.Clients.Group($"chat_{message.ChatId}")
                .SendAsync("ReceiveMessage", message);
        }

        // ----------------------------------------------------
        // MESSAGE DELETED
        // ----------------------------------------------------
        public async Task NotifyMessageDeletedAsync(int chatId, int messageId)
        {
            await _hub.Clients.Group($"chat_{chatId}")
                .SendAsync("MessageDeleted", chatId, messageId);
        }

        // ----------------------------------------------------
        // MESSAGE READ
        // ----------------------------------------------------
        public async Task NotifyMessageReadAsync(int chatId, int messageId, string userId)
        {
            await _hub.Clients.Group($"chat_{chatId}")
                .SendAsync("MessageRead", chatId, messageId, userId);
        }
    }
}
