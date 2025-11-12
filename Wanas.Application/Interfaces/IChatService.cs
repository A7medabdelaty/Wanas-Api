using Wanas.Application.DTOs.Chat;

namespace Wanas.Application.Interfaces
{
    public interface IChatService
    {
        Task<IEnumerable<ChatDto>> GetUserChatsAsync(string userId);
        Task<ChatDto> CreateChatAsync(CreateChatRequestDto request);
        Task<ChatDto?> GetChatWithMessagesAsync(int chatId);
    }
}
