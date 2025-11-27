using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Wanas.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static string GroupName(int chatId) => $"chat_{chatId}";

        // Called when a user connects to SignalR
        public override async Task OnConnectedAsync()
        {
            // Context.UserIdentifier is populated by IUserIdProvider (we register one explicitly).
            var userId = Context.UserIdentifier ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;
            await Clients.Caller.SendAsync("Connected", new { UserId = userId });
            await base.OnConnectedAsync();
        }

        // Called when a user disconnects
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier ?? Context.ConnectionId;
            // Notify all about disconnect (optional)
            await Clients.All.SendAsync("UserDisconnected", userId);
            await base.OnDisconnectedAsync(exception);
        }

        // User joins a specific chat group (chat room)
        public async Task JoinChatGroup(int chatId)
        {
            var group = GroupName(chatId);
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).SendAsync("UserJoinedChat", new { ChatId = chatId, UserId = Context.UserIdentifier ?? Context.ConnectionId });
        }

        // User leaves a chat group
        public async Task LeaveChatGroup(int chatId)
        {
            var group = GroupName(chatId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).SendAsync("UserLeftChat", new { ChatId = chatId, UserId = Context.UserIdentifier ?? Context.ConnectionId });
        }

        // Lightweight broadcast helper for clients that want to send message through hub
        // Note: prefer using the API to persist messages. If clients call this method,
        // it only broadcasts the payload to the group — persistence must be handled elsewhere.
        public async Task SendMessageToGroup(int chatId, string content)
        {
            var payload = new
            {
                ChatId = chatId,
                SenderId = Context.UserIdentifier ?? Context.ConnectionId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsTransient = true // clients can detect transient messages that may not be persisted
            };

            await Clients.Group(GroupName(chatId)).SendAsync("ReceiveMessage", payload);
        }

        // Helper for clients to notify message read locally (hub broadcasts); service-side persistence should still be invoked
        public async Task BroadcastMessageRead(int chatId, int messageId)
        {
            var userId = Context.UserIdentifier ?? Context.ConnectionId;
            await Clients.Group(GroupName(chatId)).SendAsync("MessageRead", new { ChatId = chatId, MessageId = messageId, UserId = userId });
        }
    }
}