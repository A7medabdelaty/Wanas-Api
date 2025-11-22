    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;

    namespace Wanas.API.Hubs
    {
        public class ChatHub : Hub
        {
            // Called when a user connects to SignalR
            public override async Task OnConnectedAsync()
            {
                var userId = Context.UserIdentifier ?? Context.ConnectionId;
                await Clients.Caller.SendAsync("Connected", $"Welcome user: {userId}");
                await base.OnConnectedAsync();
            }

            // Called when a user disconnects
            public override async Task OnDisconnectedAsync(Exception? exception)
            {
                var userId = Context.UserIdentifier ?? Context.ConnectionId;
                await Clients.All.SendAsync("UserDisconnected", userId);
                await base.OnDisconnectedAsync(exception);
            }

            // User joins a specific chat group (chat room)
            public async Task JoinChatGroup(string chatId)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
                await Clients.Group(chatId).SendAsync("UserJoinedChat", chatId, Context.ConnectionId);
            }

            // User leaves a chat group
            public async Task LeaveChatGroup(string chatId)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
                await Clients.Group(chatId).SendAsync("UserLeftChat", chatId, Context.ConnectionId);
            }

            // Send message to everyone in the chat
            public async Task SendMessage(string chatId, string senderId, string message)
            {
                var messageData = new
                {
                    ChatId = chatId,
                    SenderId = senderId,
                    Content = message,
                    SentAt = DateTime.UtcNow
                };

                // Broadcast to all users in that chat
                await Clients.Group(chatId).SendAsync("ReceiveMessage", messageData);
            }
        }
    }