using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Wanas.Application.Interfaces;

namespace Wanas.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static string GroupName(int chatId) => $"chat_{chatId}";

        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier!;

            // Auto-join all chat groups for this user
            var chatIds = await _chatService.GetUserChatIdsAsync(userId);

            foreach (var chatId in chatIds)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(chatId));
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        // Manually join a chat (optional)
        public async Task JoinChatGroup(int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(chatId));
        }

        // Manually leave a chat
        public async Task LeaveChatGroup(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(chatId));
        }

        // Optimistic UI helper - does NOT save to DB
        public async Task SendMessageToGroup(int chatId, string content)
        {
            var payload = new
            {
                ChatId = chatId,
                SenderId = Context.UserIdentifier,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsTransient = true // UI should ignore if API returns real message
            };

            await Clients.Group(GroupName(chatId)).SendAsync("ReceiveMessage", payload);
        }

        public async Task BroadcastMessageRead(int chatId, int messageId)
        {
            var userId = Context.UserIdentifier!;

            await Clients.Group(GroupName(chatId))
                .SendAsync("MessageRead", new { ChatId = chatId, MessageId = messageId, UserId = userId });
        }
    }
}
