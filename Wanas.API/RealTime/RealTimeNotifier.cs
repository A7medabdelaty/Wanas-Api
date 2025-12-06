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
        private readonly ILogger<RealTimeNotifier> _logger;

        public RealTimeNotifier(IHubContext<ChatHub> hub, ILogger<RealTimeNotifier> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        // CHAT CREATED
        public async Task NotifyChatCreatedAsync(ChatDto chat)
        {
            try
            {
                foreach (var participant in chat.Participants)
                {
                    await _hub.Clients.User(participant.UserId)
                        .SendAsync("ChatCreated", chat);
                    
                    _logger.LogDebug("Chat created notification sent to user {UserId} for chat {ChatId}", 
                        participant.UserId, chat.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying chat created for chat {ChatId}", chat.Id);
            }
        }

        // CHAT UPDATED
        public async Task NotifyChatUpdatedAsync(ChatDto chat)
        {
            try
            {
                await _hub.Clients.Group($"chat_{chat.Id}")
                    .SendAsync("ChatUpdated", chat);
                
                _logger.LogDebug("Chat updated notification sent to group chat_{ChatId}", chat.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying chat updated for chat {ChatId}", chat.Id);
            }
        }

        // CHAT DELETED
        public async Task NotifyChatDeletedAsync(int chatId)
        {
            try
            {
                await _hub.Clients.Group($"chat_{chatId}")
                    .SendAsync("ChatDeleted", chatId);
                
                _logger.LogDebug("Chat deleted notification sent to group chat_{ChatId}", chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying chat deleted for chat {ChatId}", chatId);
            }
        }

        // PARTICIPANT ADDED
        public async Task NotifyParticipantAddedAsync(int chatId, string userId)
        {
            try
            {
                // Notify all participants in the group
                await _hub.Clients.Group($"chat_{chatId}")
                    .SendAsync("ParticipantAdded", chatId, userId);
                
                _logger.LogInformation("Participant {UserId} added notification sent to group chat_{ChatId}", userId, chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying participant added for chat {ChatId}, user {UserId}", chatId, userId);
            }
        }

        // PARTICIPANT REMOVED
        public async Task NotifyParticipantRemovedAsync(int chatId, string userId)
        {
            try
            {
                // Notify all participants in the group
                await _hub.Clients.Group($"chat_{chatId}")
                    .SendAsync("ParticipantRemoved", chatId, userId);
                
                _logger.LogInformation("Participant {UserId} removed notification sent to group chat_{ChatId}", userId, chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying participant removed for chat {ChatId}, user {UserId}", chatId, userId);
            }
        }

        // MESSAGE RECEIVED - Notifies ALL participants (online & offline)
        public async Task NotifyMessageReceivedAsync(MessageDto message)
        {
            try
            {
                // Broadcast to all connected participants in the group
                await _hub.Clients.Group($"chat_{message.ChatId}")
                    .SendAsync("ReceiveMessage", message);
                
                _logger.LogInformation("Message received notification sent to group chat_{ChatId}. MessageId: {MessageId}, SenderId: {SenderId}", 
                    message.ChatId, message.Id, message.SenderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying message received for chat {ChatId}, message {MessageId}", 
                    message.ChatId, message.Id);
            }
        }

        // MESSAGE DELETED
        public async Task NotifyMessageDeletedAsync(int chatId, int messageId)
        {
            try
            {
                await _hub.Clients.Group($"chat_{chatId}")
                    .SendAsync("MessageDeleted", chatId, messageId);
                
                _logger.LogInformation("Message deleted notification sent to group chat_{ChatId}. MessageId: {MessageId}", 
                    chatId, messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying message deleted for chat {ChatId}, message {MessageId}", chatId, messageId);
            }
        }

        // MESSAGE READ
        public async Task NotifyMessageReadAsync(int chatId, int messageId, string userId)
        {
            try
            {
                await _hub.Clients.Group($"chat_{chatId}")
                    .SendAsync("MessageRead", chatId, messageId, userId);
                
                _logger.LogDebug("Message read notification sent to group chat_{ChatId}. MessageId: {MessageId}, UserId: {UserId}", 
                    chatId, messageId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying message read for chat {ChatId}, message {MessageId}, user {UserId}", 
                    chatId, messageId, userId);
            }
        }

        public async Task NotifyPaymentApprovedAsync(int listingId, string userId)
        {
            await _hub.Clients.User(userId)
                .SendAsync("PaymentApproved", new { ListingId = listingId });
        }
        public async Task NotifyGroupApprovedAsync(int chatId, string userId)
        {
            await _hub.Clients.Group($"chat_{chatId}")
                .SendAsync("GroupJoinApproved", new { ChatId = chatId, UserId = userId });
        }

        public async Task NotifyOwnerAsync(string ownerId, string message)
        {
            await _hub.Clients.User(ownerId).SendAsync("OwnerNotification", new
            {
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyUserAsync(string userId, string message)
        {
            await _hub.Clients.User(userId).SendAsync("UserNotification", new
            {
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }

        // USER TYPING
        public async Task NotifyUserTypingAsync(int chatId, string userId, string userName)
        {
            try
            {
                await _hub.Clients.Group($"chat_{chatId}")
                    .SendAsync("UserTyping", new { ChatId = chatId, UserId = userId, UserName = userName });

                _logger.LogDebug("User typing notification sent to group chat_{ChatId} for user {UserId}", chatId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying user typing for chat {ChatId}, user {UserId}", chatId, userId);
            }
        }
        // USER STOPPED TYPING
        public async Task NotifyUserStoppedTypingAsync(int chatId, string userId)
        {
            try
            {
                await _hub.Clients.Group($"chat_{chatId}")
                    .SendAsync("UserStoppedTyping", new { ChatId = chatId, UserId = userId });

                _logger.LogDebug("User stopped typing notification sent to group chat_{ChatId} for user {UserId}", chatId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying user stopped typing for chat {ChatId}, user {UserId}", chatId, userId);
            }
        }

        // USER STATUS CHANGED (Online/Offline)
        public async Task NotifyUserStatusChangedAsync(string userId, bool isOnline)
        {
            try
            {
                await _hub.Clients.All
                    .SendAsync("UserStatusChanged", new { UserId = userId, IsOnline = isOnline, Timestamp = DateTime.UtcNow });

                _logger.LogInformation("User status changed notification sent for user {UserId}: {Status}", userId, isOnline ? "Online" : "Offline");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying user status changed for user {UserId}", userId);
            }
        }

        // LISTING UPDATED
        public async Task NotifyListingUpdatedAsync(int listingId, string ownerId)
        {
            try
            {
                await _hub.Clients.User(ownerId)
                    .SendAsync("ListingUpdated", new { ListingId = listingId, Timestamp = DateTime.UtcNow });

                _logger.LogInformation("Listing updated notification sent to owner {OwnerId} for listing {ListingId}", ownerId, listingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying listing updated for listing {ListingId}, owner {OwnerId}", listingId, ownerId);
            }
        }
        // RESERVATION CREATED
        public async Task NotifyReservationCreatedAsync(int reservationId, string renterId, string ownerId)
        {
            try
            {
                var notificationData = new { ReservationId = reservationId, Timestamp = DateTime.UtcNow };
                // Notify the renter
                await _hub.Clients.User(renterId)
                    .SendAsync("ReservationCreated", notificationData);
                // Notify the owner
                await _hub.Clients.User(ownerId)
                    .SendAsync("ReservationCreated", notificationData);

                _logger.LogInformation("Reservation created notification sent for reservation {ReservationId} to renter {RenterId} and owner {OwnerId}",
                    reservationId, renterId, ownerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying reservation created for reservation {ReservationId}", reservationId);
            }
        }
        // RESERVATION UPDATED
        public async Task NotifyReservationUpdatedAsync(int reservationId, string userId)
        {
            try
            {
                await _hub.Clients.User(userId)
                    .SendAsync("ReservationUpdated", new { ReservationId = reservationId, Timestamp = DateTime.UtcNow });

                _logger.LogInformation("Reservation updated notification sent to user {UserId} for reservation {ReservationId}", userId, reservationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying reservation updated for reservation {ReservationId}, user {UserId}", reservationId, userId);
            }
        }
        // RESERVATION CANCELLED
        public async Task NotifyReservationCancelledAsync(int reservationId, string userId)
        {
            try
            {
                await _hub.Clients.User(userId)
                    .SendAsync("ReservationCancelled", new { ReservationId = reservationId, Timestamp = DateTime.UtcNow });

                _logger.LogInformation("Reservation cancelled notification sent to user {UserId} for reservation {ReservationId}", userId, reservationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying reservation cancelled for reservation {ReservationId}, user {UserId}", reservationId, userId);
            }
        }

    }
}
