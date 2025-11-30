using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<ChatDto>> GetUserChatsAsync(string userId)
        {
            var chats = await _uow.Chats.GetUserChatsAsync(userId);
            var result = _mapper.Map<IEnumerable<ChatDto>>(chats);

            foreach (var chat in result)
            {
                if (!chat.IsGroup && chat.Participants.Count == 2)
                {
                    var other = chat.Participants.First(x => x.UserId != userId);
                    chat.ChatName = !string.IsNullOrWhiteSpace(other.DisplayName)
                        ? other.DisplayName
                        : other.UserName;
                }
            }

            return result;
        }

        public async Task<ChatDto> GetOrCreatePrivateChatAsync(string userId, string ownerId)
        {
            var existing = await _uow.Chats.GetPrivateChatBetweenAsync(userId, ownerId);
            if (existing != null)
            {
                var fullExisting = await _uow.Chats.GetChatWithUsersAsync(existing.Id);

                var dto = _mapper.Map<ChatDto>(fullExisting);
                ApplyPrivateChatName(dto, userId);
                return dto;
            }

            // Create chat
            var chat = new Chat
            {
                IsGroup = false,
                CreatedAt = DateTime.UtcNow
            };

            chat.ChatParticipants.Add(new ChatParticipant { UserId = userId });
            chat.ChatParticipants.Add(new ChatParticipant { UserId = ownerId });

            await _uow.Chats.AddAsync(chat);
            await _uow.CommitAsync();

            var fullChat = await _uow.Chats.GetChatWithUsersAsync(chat.Id);

            var createdDto = _mapper.Map<ChatDto>(fullChat);
            ApplyPrivateChatName(createdDto, userId);
            return createdDto;
        }

        private void ApplyPrivateChatName(ChatDto dto, string userId)
        {
            if (!dto.IsGroup && dto.Participants.Count == 2)
            {
                var other = dto.Participants.First(x => x.UserId != userId);
                dto.ChatName = !string.IsNullOrWhiteSpace(other.DisplayName)
                    ? other.DisplayName
                    : other.UserName;
            }
        }

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

        public async Task<ChatDto> CreateChatAsync(string creatorId, CreateChatRequestDto request)
        {
            var chat = new Chat
            {
                Name = request.ChatName,
                IsGroup = true,
                CreatedAt = DateTime.UtcNow
            };

            chat.ChatParticipants.Add(new ChatParticipant { UserId = creatorId });

            await _uow.Chats.AddAsync(chat);
            await _uow.CommitAsync();

            var dto = _mapper.Map<ChatDto>(chat);
            await _notifier.NotifyChatCreatedAsync(dto);

            return dto;
        }

        public async Task<bool> AddParticipantAsync(int chatId, string userId)
        {
            var chat = await _uow.Chats.GetChatWithParticipantsAsync(chatId);
            if (chat == null)
                return false;

            if (chat.ChatParticipants.Any(p => p.UserId == userId))
                return false;

            var participant = new ChatParticipant { ChatId = chatId, UserId = userId };
            await _uow.ChatParticipants.AddAsync(participant);
            await _uow.CommitAsync();

            await _notifier.NotifyParticipantAddedAsync(chatId, userId);
            return true;
        }

        public async Task<bool> RemoveParticipantAsync(int chatId, string userId)
        {
            var participant = (await _uow.ChatParticipants
                .FindAsync(p => p.ChatId == chatId && p.UserId == userId))
                .FirstOrDefault();

            if (participant == null)
                return false;

            _uow.ChatParticipants.Remove(participant);
            await _uow.CommitAsync();

            await _notifier.NotifyParticipantRemovedAsync(chatId, userId);
            return true;
        }

        public async Task<ChatDto?> UpdateChatAsync(int chatId, UpdateChatRequestDto request)
        {
            var chat = await _uow.Chats.GetByIdAsync(chatId);
            if (chat == null)
                return null;

            if (!string.IsNullOrWhiteSpace(request.NewName))
                chat.Name = request.NewName;

            chat.UpdatedAt = DateTime.UtcNow;

            _uow.Chats.Update(chat);
            await _uow.CommitAsync();

            var dto = _mapper.Map<ChatDto>(chat);

            // Dedicated notifier for updates
            await _notifier.NotifyChatUpdatedAsync(dto);

            return dto;
        }

        public async Task<IList<int>> GetUserChatIdsAsync(string userId)
        {
            var list = await _uow.ChatParticipants.GetAllAsync();
            return list.Where(cp => cp.UserId == userId && cp.LeftAt == null)
                .Select(cp => cp.ChatId).ToList();
        }

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

        public async Task<bool> LeaveChatAsync(int chatId, string userId)
        {
            var participant = (await _uow.ChatParticipants
                .FindAsync(p => p.ChatId == chatId && p.UserId == userId))
                .FirstOrDefault();

            if (participant == null)
                return false;

            _uow.ChatParticipants.Remove(participant);
            await _uow.CommitAsync();

            await _notifier.NotifyParticipantRemovedAsync(chatId, userId);
            return true;
        }

        public async Task<bool> MarkChatAsReadAsync(int chatId, string userId)
        {
            var messages = await _uow.Messages
                .FindAsync(m => m.ChatId == chatId && m.SenderId != userId);

            foreach (var msg in messages)
            {
                if (!msg.ReadReceipts.Any(r => r.UserId == userId))
                {
                    msg.ReadReceipts.Add(new MessageReadReceipt
                    {
                        MessageId = msg.Id,
                        UserId = userId,
                        ReadAt = DateTime.UtcNow
                    });
                }
            }

            await _uow.CommitAsync();
            return true;
        }

        public async Task<int> GetUnreadMessagesCountAsync(string userId)
        {
            var messages = await _uow.Messages.FindAsync(
                m => m.SenderId != userId &&
                !m.ReadReceipts.Any(r => r.UserId == userId));

            return messages.Count();
        }

        public async Task<IEnumerable<ChatSummaryDto>> GetRecentChatsAsync(string userId)
        {
            var chats = await _uow.Chats.GetUserChatsAsync(userId);

            return chats
                .Select(c =>
                {
                    var last = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
                    var unread = c.Messages.Count(
                        m => m.SenderId != userId &&
                        !m.ReadReceipts.Any(r => r.UserId == userId));

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
        }
    }
}
