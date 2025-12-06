using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Wanas.Application.DTOs.Message;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;

namespace Wanas.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static string GroupName(int chatId) => $"chat_{chatId}";
        private readonly IChatService _chatService;
        private readonly IMessageService _messageService;
        private readonly IRealTimeNotifier _notifier;
        private readonly ILogger<ChatHub> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(
            IChatService chatService,
            IMessageService messageService,
            IRealTimeNotifier notifier,
            ILogger<ChatHub> logger,
            UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _messageService = messageService;
            _notifier = notifier;
            _logger = logger;
            _userManager = userManager;
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
                // Notify user status changed (online)
                await _notifier.NotifyUserStatusChangedAsync(userId, true);
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

                    // Notify user status changed (offline)
                    await _notifier.NotifyUserStatusChangedAsync(userId, false);
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
                // Send real-time notification via RealTimeNotifier
                // This will broadcast to all participants in the chat group
                await _notifier.NotifyMessageReceivedAsync(message);
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
        // Broadcast message read status
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
                // Send notification via RealTimeNotifier
                await _notifier.NotifyMessageReadAsync(chatId, messageId, userId);
                _logger.LogDebug("Message read broadcast for MessageId {MessageId} in ChatId {ChatId} by user {UserId}", messageId, chatId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BroadcastMessageRead for ChatId: {ChatId}, MessageId: {MessageId}", chatId, messageId);
                throw new HubException("Failed to broadcast message read status");
            }
        }
        // Typing indicators - New methods
        public async Task NotifyTyping(int chatId)
        {
            try
            {
                var userId = Context.UserIdentifier;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("NotifyTyping attempted without valid user. ConnectionId: {ConnectionId}", Context.ConnectionId);
                    throw new HubException("User not authenticated");
                }
                // Get user name from your user service/database
                // For now, using userId as userName - you should replace this with actual user name lookup
                var user = await _userManager.FindByIdAsync(userId);
                var userName = user?.FullName;
                await _notifier.NotifyUserTypingAsync(chatId, userId, userName);
                _logger.LogDebug("User {UserId} started typing in chat {ChatId}", userId, chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NotifyTyping for ChatId: {ChatId}", chatId);
                throw new HubException("Failed to notify typing");
            }
        }
        public async Task NotifyStoppedTyping(int chatId)
        {
            try
            {
                var userId = Context.UserIdentifier;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("NotifyStoppedTyping attempted without valid user. ConnectionId: {ConnectionId}", Context.ConnectionId);
                    throw new HubException("User not authenticated");
                }
                await _notifier.NotifyUserStoppedTypingAsync(chatId, userId);
                _logger.LogDebug("User {UserId} stopped typing in chat {ChatId}", userId, chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NotifyStoppedTyping for ChatId: {ChatId}", chatId);
                throw new HubException("Failed to notify stopped typing");
            }
        }
    }
}
