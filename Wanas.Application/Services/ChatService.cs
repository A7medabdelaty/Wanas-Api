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

        public ChatService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<IEnumerable<ChatDto>> GetUserChatsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required.", nameof(userId));

            var chats = await _uow.Chats.GetUserChatsAsync(userId);
            return _mapper.Map<IEnumerable<ChatDto>>(chats);
        }
        public async Task<ChatDto?> GetChatWithMessagesAsync(int chatId)
        {
            var chat = await _uow.Chats.GetChatWithMessagesAsync(chatId);
            return chat == null ? null : _mapper.Map<ChatDto>(chat);
        }
        public async Task<ChatDto> CreateChatAsync(CreateChatRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.UserId))
                throw new ArgumentException("UserId is required.", nameof(request.UserId));

            var chat = new Chat
            {
                Name = request.ChatName,
                IsGroup = request.IsGroup,
                CreatedAt = DateTime.UtcNow
            };

            var participant = new ChatParticipant
            {
                UserId = request.UserId,
                Chat = chat
            };

            chat.ChatParticipants.Add(participant);

            await _uow.Chats.AddAsync(chat);
            await _uow.CommitAsync();

            return _mapper.Map<ChatDto>(chat);
        }
        public async Task<bool> AddParticipantAsync(AddParticipantRequestDto request)
        {
            var chat = await _uow.Chats.GetChatWithParticipantsAsync(request.ChatId);
            if (chat == null)
                throw new InvalidOperationException("Chat not found.");

            // Prevent duplicates
            if (chat.ChatParticipants.Any(p => p.UserId == request.UserId))
                return false;

            var participant = new ChatParticipant
            {
                ChatId = request.ChatId,
                UserId = request.UserId
            };

            await _uow.ChatParticipants.AddAsync(participant);
            await _uow.CommitAsync();
            return true;
        }
        public async Task<bool> RemoveParticipantAsync(int chatId, string userId)
        {
            var participant = await _uow.ChatParticipants.GetParticipantAsync(chatId, userId);
            if (participant == null)
                return false; // not found or already removed

            _uow.ChatParticipants.Remove(participant);
            await _uow.CommitAsync();
            return true;
        }
    }
}
