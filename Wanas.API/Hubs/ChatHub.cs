using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Wanas.Application.DTOs.Message;
using Wanas.Application.Interfaces;

namespace Wanas.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static string GroupName(int chatId) => $"chat_{chatId}";

        private readonly IChatService _chatService;
        private readonly IMessageService _messageService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatService chatService, IMessageService messageService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _messageService = messageService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.UserIdentifier;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Connection attempted without valid user identifier. ConnectionId: {ConnectionId}", Context.ConnectionId);
                    await base.OnConnectedAsync();
                    return;
                }

                _logger.LogInformation("User {UserId} connected to ChatHub. ConnectionId: {ConnectionId}", userId, Context.ConnectionId);

                // Auto-join all chat groups for this user
                var chatIds = await _chatService.GetUserChatIdsAsync(userId);

                foreach (var chatId in chatIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(chatId));
                    _logger.LogDebug("User {UserId} added to group {GroupName}", userId, GroupName(chatId));
                }

                _logger.LogInformation("User {UserId} successfully joined {ChatCount} chat groups", userId, chatIds.Count());

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync for ConnectionId: {ConnectionId}", Context.ConnectionId);
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var userId = Context.UserIdentifier;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Disconnection for unknown user. ConnectionId: {ConnectionId}", Context.ConnectionId);
                }
                else
                {
                    _logger.LogInformation("User {UserId} disconnected from ChatHub. ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
                }

                if (exception != null)
                {
                    _logger.LogError(exception, "Disconnection error for user {UserId}. ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync for ConnectionId: {ConnectionId}", Context.ConnectionId);
            }
        }

        // Manually join a chat (optional)
        public async Task JoinChatGroup(int chatId)
        {
            try
            {
                var userId = Context.UserIdentifier;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("JoinChatGroup attempted without valid user. ConnectionId: {ConnectionId}", Context.ConnectionId);
                    throw new HubException("User not authenticated");
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(chatId));
                _logger.LogInformation("User {UserId} manually joined chat group {ChatId}", userId, chatId);

                // Notify others in the group
                await Clients.OthersInGroup(GroupName(chatId))
                    .SendAsync("UserJoinedChat", new { UserId = userId, ChatId = chatId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JoinChatGroup for ChatId: {ChatId}", chatId);
                throw new HubException("Failed to join chat group");
            }
        }

        // Manually leave a chat
        public async Task LeaveChatGroup(int chatId)
        {
            try
            {
                var userId = Context.UserIdentifier;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("LeaveChatGroup attempted without valid user. ConnectionId: {ConnectionId}", Context.ConnectionId);
                    throw new HubException("User not authenticated");
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(chatId));
                _logger.LogInformation("User {UserId} manually left chat group {ChatId}", userId, chatId);

                // Notify others in the group
                await Clients.OthersInGroup(GroupName(chatId))
                    .SendAsync("UserLeftChat", new { UserId = userId, ChatId = chatId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LeaveChatGroup for ChatId: {ChatId}", chatId);
                throw new HubException("Failed to leave chat group");
            }
        }

        // Send message - Saves to DB and broadcasts to group
        public async Task SendMessageToGroup(int chatId, string content)
        {
            try
            {
                var userId = Context.UserIdentifier;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("SendMessageToGroup attempted without valid user. ConnectionId: {ConnectionId}", Context.ConnectionId);
                    throw new HubException("User not authenticated");
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("SendMessageToGroup called with empty content by user {UserId}", userId);
                    throw new HubException("Message content cannot be empty");
                }

                // Create message DTO for service
                var createMessageDto = new CreateMessageRequestDto
                {
                    ChatId = chatId,
                    SenderId = userId,
                    Content = content
                };

                // Save to database and get persisted message
                var message = await _messageService.SendMessageAsync(createMessageDto);

                _logger.LogInformation("Message saved and broadcast to group {GroupName} by user {UserId}", GroupName(chatId), userId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in SendMessageToGroup: {Message}", ex.Message);
                throw new HubException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessageToGroup for ChatId: {ChatId}", chatId);
                throw new HubException("Failed to send message");
            }
        }

        public async Task BroadcastMessageRead(int chatId, int messageId)
        {
            try
            {
                var userId = Context.UserIdentifier;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("BroadcastMessageRead attempted without valid user. ConnectionId: {ConnectionId}", Context.ConnectionId);
                    throw new HubException("User not authenticated");
                }

                await Clients.Group(GroupName(chatId))
                    .SendAsync("MessageRead", new { ChatId = chatId, MessageId = messageId, UserId = userId });

                _logger.LogDebug("Message read broadcast for MessageId {MessageId} in ChatId {ChatId} by user {UserId}", messageId, chatId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BroadcastMessageRead for ChatId: {ChatId}, MessageId: {MessageId}", chatId, messageId);
                throw new HubException("Failed to broadcast message read status");
            }
        }
    }
}
