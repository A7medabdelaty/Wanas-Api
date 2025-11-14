using Wanas.Application.DTOs.Message;

namespace Wanas.Application.Interfaces
{
    public interface IMessageService
    {
        Task<IEnumerable<MessageDto>> GetMessagesByChatAsync(int chatId, int limit = 50);
        Task<MessageDto> SendMessageAsync(CreateMessageRequestDto request);
        Task<bool> DeleteMessageAsync(int messageId);
        Task<bool> EditMessageAsync(int messageId, string newContent);
        Task<bool> MarkMessageAsReadAsync(int messageId, string userId);
    }
}
