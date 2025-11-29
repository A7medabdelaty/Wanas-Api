using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
        private readonly IRealTimeNotifier _notifier;

        public MessageService(IUnitOfWork uow, IMapper mapper, IRealTimeNotifier notifier)
        {
            _uow = uow;
            _mapper = mapper;
            _notifier = notifier;
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesByChatAsync(int chatId, int limit = 50)
        {
            var messages = await _uow.Messages.GetMessagesByChatIdAsync(chatId);
            var ordered = messages.OrderByDescending(m => m.SentAt).Take(limit).OrderBy(m => m.SentAt).ToList();
            return _mapper.Map<IEnumerable<MessageDto>>(ordered);
        }

        public async Task<MessageDto> SendMessageAsync(CreateMessageRequestDto request)
        {
            var chat = await _uow.Chats.GetChatWithParticipantsAsync(request.ChatId);
            if (chat == null)
                throw new InvalidOperationException("Chat not found.");
            if (!chat.ChatParticipants.Any(p => p.UserId == request.SenderId))
                throw new InvalidOperationException("Sender is not a participant.");

            var message = new Message
            {
                ChatId = request.ChatId,
                SenderId = request.SenderId,
                TextContent = request.Content,
                SentAt = DateTime.UtcNow
            };

            await _uow.Messages.AddAsync(message);
            await _uow.CommitAsync();

            var dto = _mapper.Map<MessageDto>(message);

            // broadcast to group
            await _notifier.NotifyMessageReceivedAsync(dto);

            return dto;
        }

        public async Task<bool> DeleteMessageAsync(int messageId, string userId)
        {
            var msg = await _uow.Messages.GetByIdAsync(messageId);
            if (msg == null)
                return false;

            // Only sender can delete
            if (msg.SenderId != userId)
                return false;

            var chatId = msg.ChatId;

            _uow.Messages.Remove(msg);
            await _uow.CommitAsync();

            await _notifier.NotifyMessageDeletedAsync(chatId, messageId);
            return true;
        }

        public async Task<bool> EditMessageAsync(int messageId, string newContent, string userId)
        {
            var msg = await _uow.Messages.GetByIdAsync(messageId);
            if (msg == null)
                return false;

            // Only sender can edit their own messages
            if (msg.SenderId != userId)
                return false;

            msg.TextContent = newContent;
            msg.IsEdited = true;

            _uow.Messages.Update(msg);
            await _uow.CommitAsync();

            var dto = _mapper.Map<MessageDto>(msg);

            return true;
        }

        public async Task<bool> MarkMessageAsReadAsync(int messageId, string userId)
        {
            var msg = await _uow.Messages.GetByIdAsync(messageId);
            if (msg == null)
                return false;

            // First attempt an efficient DB-side check to see if a receipt already exists.
            var existing = (await _uow.Messages.FindAsync(m =>
                            m.Id == messageId && m.ReadReceipts.Any(r => r.UserId == userId)))
                           .Any();

            if (existing)
                return true;

            // Add the receipt locally and try to commit.
            msg.ReadReceipts.Add(new MessageReadReceipt
            {
                MessageId = messageId,
                UserId = userId,
                ReadAt = DateTime.UtcNow
            });

            try
            {
                await _uow.CommitAsync();
            }
            catch (DbUpdateException)
            {
                return true;
            }

            await _notifier.NotifyMessageReadAsync(msg.ChatId, messageId, userId);

            return true;
        }
    }
}
