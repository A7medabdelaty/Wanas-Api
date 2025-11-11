using Wanas.Application.DTOs.Message;

namespace Wanas.Application.Interfaces
{
    public interface IMessageService
    {
        Task<IEnumerable<MessageDto>> GetMessagesByChatAsync(int chatId, int limit = 50);
        Task<MessageDto> SendMessageAsync(CreateMessageRequestDto request);
    }
}
