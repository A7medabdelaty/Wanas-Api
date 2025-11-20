using AutoMapper;
using Wanas.Application.DTOs.Chat;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _uow;
        private readonly IMapper _mapper;
        private readonly IRealTimeNotifier _notifier;

        public ChatService(AppDbContext uow, IMapper mapper, IRealTimeNotifier notifier)
        {
            _uow = uow;
            _mapper = mapper;
            _notifier = notifier;
        }

        public async Task<IEnumerable<ChatDto>> GetUserChatsAsync(string userId)
        {
            var chats = await _uow.Chats.GetUserChatsAsync(userId);
            return _mapper.Map<IEnumerable<ChatDto>>(chats);
        }

        public async Task<ChatDto?> GetChatDetailsAsync(int chatId)
        {
            var chat = await _uow.Chats.GetChatWithMessagesAsync(chatId);
            return chat == null ? null : _mapper.Map<ChatDto>(chat);
        }

        public async Task<ChatDto> CreateChatAsync(CreateChatRequestDto request)
        {
            var chat = new Chat
            {
                Name = request.ChatName,
                IsGroup = request.IsGroup,
                CreatedAt = DateTime.UtcNow
            };

            chat.ChatParticipants.Add(new ChatParticipant { UserId = request.UserId });

            await _uow.Chats.AddAsync(chat);
            await _uow.CommitAsync();

            var dto = _mapper.Map<ChatDto>(chat);

            // notify
            await _notifier.NotifyChatCreatedAsync(dto);

            return dto;
        }

        public async Task<bool> AddParticipantAsync(AddParticipantRequestDto request)
        {
            var chat = await _uow.Chats.GetChatWithParticipantsAsync(request.ChatId);
            if (chat == null)
                return false;

            if (chat.ChatParticipants.Any(p => p.UserId == request.UserId))
                return false;

            var participant = new ChatParticipant { ChatId = request.ChatId, UserId = request.UserId };
            await _uow.ChatParticipants.AddAsync(participant);
            await _uow.CommitAsync();

            await _notifier.NotifyParticipantAddedAsync(request.ChatId, request.UserId);
            return true;
        }

        public async Task<bool> RemoveParticipantAsync(int chatId, string userId)
        {
            var participant = (await _uow.ChatParticipants.FindAsync(p => p.ChatId == chatId && p.UserId == userId))
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
            if (request.IsGroup.HasValue)
                chat.IsGroup = request.IsGroup.Value;
            chat.UpdatedAt = DateTime.UtcNow;

            _uow.Chats.Update(chat);
            await _uow.CommitAsync();

            var dto = _mapper.Map<ChatDto>(chat);
            // Optionally notify about update:
            await _notifier.NotifyChatCreatedAsync(dto); // re-use ChatCreated to inform clients of update
            return dto;
        }

        public async Task<bool> DeleteChatAsync(int chatId)
        {
            var chat = await _uow.Chats.GetByIdAsync(chatId);
            if (chat == null)
                return false;

            _uow.Chats.Remove(chat);
            await _uow.CommitAsync();
            // Optionally: notify clients chat deleted (you can add another notifier method)
            return true;
        }

        public async Task<bool> LeaveChatAsync(int chatId, string userId)
        {
            var participant = (await _uow.ChatParticipants.FindAsync(p => p.ChatId == chatId && p.UserId == userId))
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
            var messages = await _uow.Messages.FindAsync(m => m.ChatId == chatId && m.SenderId != userId);
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
            // Optionally notify clients about read receipts
            return true;
        }

        public async Task<int> GetUnreadMessagesCountAsync(string userId)
        {
            var messages = await _uow.Messages.FindAsync(m => m.SenderId != userId &&
                !m.ReadReceipts.Any(r => r.UserId == userId));
            return messages.Count();
        }

        public async Task<IEnumerable<ChatSummaryDto>> GetRecentChatsAsync(string userId)
        {
            var chats = await _uow.Chats.GetUserChatsAsync(userId);

            var summaries = chats.Select(c =>
            {
                var last = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
                var unread = c.Messages.Count(m => m.SenderId != userId && !m.ReadReceipts.Any(r => r.UserId == userId));
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
    }
}
