using Wanas.Application.DTOs.Chat;
using Wanas.Application.DTOs.Message;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _uow;

        public ChatService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<ChatDto>> GetUserChatsAsync(string userId)
        {
            var chats = await _uow.Chats.GetUserChatsAsync(userId);

            return chats.Select(c => new ChatDto
            {
                Id = c.Id,
                Name = c.Name,
                IsGroup = c.IsGroup,
                CreatedAt = c.CreatedAt,
                ParticipantIds = c.ChatParticipants.Select(p => p.UserId).ToList()
            });
        }

        public async Task<ChatDto?> GetChatDetailsAsync(int chatId)
        {
            var chat = await _uow.Chats.GetChatWithMessagesAsync(chatId);
            if (chat == null)
                return null;

            return new ChatDto
            {
                Id = chat.Id,
                Name = chat.Name,
                IsGroup = chat.IsGroup,
                CreatedAt = chat.CreatedAt,
                ParticipantIds = chat.ChatParticipants.Select(p => p.UserId).ToList(),
                Messages = chat.Messages.Select(m => new MessageDto
                {
                    Id = m.Id,
                    ChatId = m.ChatId,
                    SenderId = m.SenderId,
                    Content = m.TextContent,
                    SentAt = m.SentAt
                })
            };
        }

        public async Task<int> CreateChatAsync(string userId, string? chatName = null, bool isGroup = false)
        {
            var chat = new Chat
            {
                Name = chatName,
                IsGroup = isGroup,
                CreatedAt = DateTime.UtcNow,
                ChatParticipants = new List<ChatParticipant>
                {
                    new ChatParticipant { UserId = userId }
                }
            };

            await _uow.Chats.AddAsync(chat);
            await _uow.CommitAsync();
            return chat.Id;
        }
    }
}
