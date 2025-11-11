using AutoMapper;
using Wanas.Application.DTOs.Message;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public MessageService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // Returns latest messages (ordered descending by SentAt)
        public async Task<IEnumerable<MessageDto>> GetMessagesByChatAsync(int chatId, int limit = 50)
        {
            if (limit <= 0)
                limit = 50;
            var messages = await _uow.Messages.GetMessagesByChatIdAsync(chatId);
            // Repository returns (likely) all or last N; ensure ordering and take here:
            var ordered = messages
                .OrderByDescending(m => m.SentAt)
                .Take(limit)
                .OrderBy(m => m.SentAt) // return ascending so client can render oldest->newest
                .ToList();

            return _mapper.Map<IEnumerable<MessageDto>>(ordered);
        }

        public async Task<MessageDto> SendMessageAsync(CreateMessageRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.SenderId))
                throw new ArgumentException("SenderId is required.", nameof(request.SenderId));
            if (string.IsNullOrWhiteSpace(request.Content))
                throw new ArgumentException("Content is required.", nameof(request.Content));

            // Validate chat exists and sender is a participant
            var chat = await _uow.Chats.GetChatWithParticipantsAsync(request.ChatId);
            if (chat == null)
                throw new InvalidOperationException("Chat not found.");

            var isParticipant = chat.ChatParticipants.Any(p => p.UserId == request.SenderId);
            if (!isParticipant)
                throw new InvalidOperationException("User is not a participant of this chat.");

            // Create message entity (adapt property names if needed)
            var message = new Message
            {
                ChatId = request.ChatId,
                SenderId = request.SenderId,
                TextContent = request.Content,
                SentAt = DateTime.UtcNow
            };

            await _uow.Messages.AddAsync(message);
            await _uow.CommitAsync(); // using domain IUnitOfWork.CommitAsync()

            // map saved message to DTO
            var dto = _mapper.Map<MessageDto>(message);
            return dto;
        }
    }
}
