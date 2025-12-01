using AutoMapper;
using Wanas.Application.DTOs.Chat;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IRealTimeNotifier _notifier;

        public ChatService(IUnitOfWork uow, IMapper mapper, IRealTimeNotifier notifier)
        {
            _uow = uow;
            _mapper = mapper;
            _notifier = notifier;
        }

        // Get all chats for a user
        public async Task<IEnumerable<ChatDto>> GetUserChatsAsync(string userId)
        {
            // Repo should return chats with participants (and optionally with last message)
            var chats = await _uow.Chats.GetUserChatsAsync(userId);

            var dtos = _mapper.Map<IEnumerable<ChatDto>>(chats).ToList();

            // Personalize 1-to-1 chat names
            foreach (var dto in dtos)
            {
                if (!dto.IsGroup && dto.Participants?.Count() == 2)
                    ApplyPrivateChatName(dto, userId);
            }

            return dtos;
        }

        // Get or create a private chat between two users
        public async Task<ChatDto> GetOrCreatePrivateChatAsync(string userId, string ownerId)
        {
            // Check existing private chat (repo-level optimized query)
            var existing = await _uow.Chats.GetPrivateChatBetweenAsync(userId, ownerId);
            if (existing != null)
            {
                var fullExisting = await _uow.Chats.GetChatWithUsersAsync(existing.Id);
                var dto = _mapper.Map<ChatDto>(fullExisting);
                ApplyPrivateChatName(dto, userId);
                return dto;
            }

            // Create new private chat with initialized participants
            var chat = new Chat
            {
                IsGroup = false,
                CreatedAt = DateTime.UtcNow,
                ChatParticipants = new List<ChatParticipant>
                {
                    new ChatParticipant { UserId = userId },
                    new ChatParticipant { UserId = ownerId }
                }
            };

            await _uow.Chats.AddAsync(chat);
            await _uow.CommitAsync();

            // Reload to include user navigation properties if needed
            var fullChat = await _uow.Chats.GetChatWithUsersAsync(chat.Id);
            var createdDto = _mapper.Map<ChatDto>(fullChat);
            ApplyPrivateChatName(createdDto, userId);
            // Optionally notify only participants (owner + requester)
            await _notifier.NotifyChatCreatedAsync(createdDto);
            return createdDto;
        }

        // Get chat details (with messages).
        public async Task<ChatDto?> GetChatDetailsAsync(int chatId, string userId)
        {
            var chat = await _uow.Chats.GetChatWithMessagesAsync(chatId);
            if (chat == null)
                return null;

            var dto = _mapper.Map<ChatDto>(chat);

            if (!dto.IsGroup)
                ApplyPrivateChatName(dto, userId);

            return dto;
        }

        // Create a new GROUP chat
        public async Task<ChatDto> CreateChatAsync(string creatorId, CreateChatRequestDto request)
        {
            var chat = new Chat
            {
                Name = request.ChatName,
                IsGroup = true,
                CreatedAt = DateTime.UtcNow,
                ChatParticipants = new List<ChatParticipant>
                {
                    new ChatParticipant { UserId = creatorId, IsAdmin = true, JoinedAt = DateTime.UtcNow }
                }
            };

            await _uow.Chats.AddAsync(chat);
            await _uow.CommitAsync();

            var full = await _uow.Chats.GetChatWithUsersAsync(chat.Id);
            var dto = _mapper.Map<ChatDto>(full);

            await _notifier.NotifyChatCreatedAsync(dto);
            return dto;
        }

        // Add participant: only allowed for group chats
        public async Task<bool> AddParticipantAsync(int chatId, string userId)
        {
            var chat = await _uow.Chats.GetChatWithParticipantsAsync(chatId);
            if (chat == null)
                return false;

            if (!chat.IsGroup)
                return false; // cannot add participants to private chats

            if (chat.ChatParticipants.Any(p => p.UserId == userId && p.LeftAt == null))
                return false;

            var participant = new ChatParticipant
            {
                ChatId = chatId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };

            await _uow.ChatParticipants.AddAsync(participant);
            await _uow.CommitAsync();

            await _notifier.NotifyParticipantAddedAsync(chatId, userId);
            return true;
        }

        // Remove participant: only allowed for group chats
        public async Task<bool> RemoveParticipantAsync(int chatId, string userId)
        {
            var chat = await _uow.Chats.GetChatWithParticipantsAsync(chatId);
            if (chat == null)
                return false;

            if (!chat.IsGroup)
                return false; // avoid breaking private chats

            var participant = chat.ChatParticipants.FirstOrDefault(p => p.UserId == userId && p.LeftAt == null);
            if (participant == null)
                return false;

            _uow.ChatParticipants.Remove(participant);
            await _uow.CommitAsync();

            await _notifier.NotifyParticipantRemovedAsync(chatId, userId);
            return true;
        }

        // Update chat
        public async Task<ChatDto?> UpdateChatAsync(int chatId, UpdateChatRequestDto request)
        {
            var chat = await _uow.Chats.GetByIdAsync(chatId);
            if (chat == null)
                return null;

            if (!string.IsNullOrWhiteSpace(request.NewName) && chat.IsGroup)
                chat.Name = request.NewName;

            if (request.IsGroup.HasValue)
                chat.IsGroup = request.IsGroup.Value;

            chat.UpdatedAt = DateTime.UtcNow;
            _uow.Chats.Update(chat);
            await _uow.CommitAsync();

            var dto = _mapper.Map<ChatDto>(chat);
            await _notifier.NotifyChatUpdatedAsync(dto);
            return dto;
        }

        // Get all chat IDs for a user
        public async Task<IList<int>> GetUserChatIdsAsync(string userId)
        {
            var participants = await _uow.ChatParticipants.FindAsync(cp => cp.UserId == userId && cp.LeftAt == null);
            return participants.Select(p => p.ChatId).Distinct().ToList();
        }

        // Delete chat
        public async Task<bool> DeleteChatAsync(int chatId)
        {
            var chat = await _uow.Chats.GetByIdAsync(chatId);
            if (chat == null)
                return false;

            _uow.Chats.Remove(chat);
            await _uow.CommitAsync();

            await _notifier.NotifyChatDeletedAsync(chatId);
            return true;
        }

        // Leave chat
        public async Task<bool> LeaveChatAsync(int chatId, string userId)
        {
            var chat = await _uow.Chats.GetChatWithParticipantsAsync(chatId);
            if (chat == null)
                return false;

            if (!chat.IsGroup)
                return false;

            var participant = chat.ChatParticipants.FirstOrDefault(p => p.UserId == userId && p.LeftAt == null);
            if (participant == null)
                return false;

            // Mark left
            participant.LeftAt = DateTime.UtcNow;
            _uow.ChatParticipants.Update(participant);
            await _uow.CommitAsync();

            await _notifier.NotifyParticipantRemovedAsync(chatId, userId);
            return true;
        }

        // Mark all unseen messages in a chat as read for the current user
        public async Task<bool> MarkChatAsReadAsync(int chatId, string userId)
        {
            var messages = await _uow.Messages.FindAsync(m => m.ChatId == chatId && m.SenderId != userId);

            var changed = false;
            foreach (var msg in messages)
            {
                // Ensure read receipts collection exists
                if (msg.ReadReceipts == null)
                    msg.ReadReceipts = new List<MessageReadReceipt>();

                if (!msg.ReadReceipts.Any(r => r.UserId == userId))
                {
                    msg.ReadReceipts.Add(new MessageReadReceipt
                    {
                        MessageId = msg.Id,
                        UserId = userId,
                        ReadAt = DateTime.UtcNow
                    });
                    changed = true;
                }
            }

            if (changed)
                await _uow.CommitAsync();

            return true;
        }

        // Unread messages count for a user across all chats
        public async Task<int> GetUnreadMessagesCountAsync(string userId)
        {
            var messages = await _uow.Messages.FindAsync(m => m.SenderId != userId &&
                                                             !m.ReadReceipts.Any(r => r.UserId == userId));
            return messages.Count();
        }

        // Recent chats summary
        public async Task<IEnumerable<ChatSummaryDto>> GetRecentChatsAsync(string userId)
        {
            var chats = await _uow.Chats.GetUserChatsAsync(userId);

            var summaries = chats.Select(c =>
            {
                var last = c.Messages?.OrderByDescending(m => m.SentAt).FirstOrDefault();
                var unread = c.Messages?.Count(m => m.SenderId != userId &&
                                                   !m.ReadReceipts.Any(r => r.UserId == userId)) ?? 0;

                return new ChatSummaryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsGroup = c.IsGroup,
                    LastMessageContent = last?.TextContent,
                    LastMessageTime = last?.SentAt,
                    UnreadCount = unread
                };
            })
            .OrderByDescending(s => s.LastMessageTime)
            .ToList();

            return summaries;
        }

        // Helper: apply private chat name for the given dto
        private void ApplyPrivateChatName(ChatDto dto, string userId)
        {
            if (dto == null)
                return;
            if (dto.IsGroup)
                return;

            var participants = dto.Participants?.ToList() ?? new List<ChatParticipantDto>();
            if (participants.Count != 2)
            {
                dto.ChatName = dto.ChatName ?? $"Chat #{dto.Id}";
                return;
            }

            var other = participants.FirstOrDefault(p => !string.Equals(p.UserId, userId, StringComparison.OrdinalIgnoreCase));
            if (other == null)
            {
                dto.ChatName = dto.ChatName ?? $"Chat #{dto.Id}";
                return;
            }

            dto.ChatName = !string.IsNullOrWhiteSpace(other.DisplayName) ? other.DisplayName : other.UserName;
        }

        // get a private chat DTO without creating if missing
        public async Task<ChatDto?> GetPrivateChatIfExistsAsync(string userA, string userB)
        {
            var chat = await _uow.Chats.GetPrivateChatBetweenAsync(userA, userB);
            if (chat == null)
                return null;

            var full = await _uow.Chats.GetChatWithUsersAsync(chat.Id);
            var dto = _mapper.Map<ChatDto>(full);
            ApplyPrivateChatName(dto, userA);
            return dto;
        }

        // get private chat (create not allowed)
        public async Task<ChatDto?> GetPrivateChatAsync(string userA, string userB, int? listingId = null)
        {
            var chat = await _uow.Chats.GetPrivateChatBetweenAsync(userA, userB);

            if (chat == null)
                return null;

            // If listingId is supplied, ensure chat is related to listing (if you store that)
            if (listingId.HasValue && chat.ListingId != listingId.Value)
            {
                // If you don't tie chat to listing, skip this check
            }

            var full = await _uow.Chats.GetChatWithUsersAsync(chat.Id);
            var dto = _mapper.Map<ChatDto>(full);
            ApplyPrivateChatName(dto, userA);
            return dto;
        }

        public async Task<ChatDto> GetPrivateChatForListingAsync(string ownerId, string userId, int listingId)
        {
            // Check if a private chat exists between them
            var existing = await _uow.Chats.GetPrivateChatBetweenAsync(ownerId, userId);

            if (existing != null)
            {
                var dto = _mapper.Map<ChatDto>(existing);
                ApplyPrivateChatName(dto, ownerId);
                return dto;
            }

            // If not, create one
            var chat = new Chat
            {
                IsGroup = false,
                CreatedAt = DateTime.UtcNow
            };

            chat.ChatParticipants.Add(new ChatParticipant { UserId = ownerId });
            chat.ChatParticipants.Add(new ChatParticipant { UserId = userId });

            await _uow.Chats.AddAsync(chat);
            await _uow.CommitAsync();

            var createdDto = _mapper.Map<ChatDto>(chat);
            ApplyPrivateChatName(createdDto, ownerId);

            return createdDto;
        }
    }
}
