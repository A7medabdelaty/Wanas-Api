using Wanas.Application.DTOs.Chat;

namespace Wanas.Application.Interfaces
{
    public interface IChatService
    {
        Task<IEnumerable<ChatDto>> GetUserChatsAsync(string userId);
        Task<ChatDto?> GetChatDetailsAsync(int chatId);
        Task<int> CreateChatAsync(string userId, string? chatName = null, bool isGroup = false);
    }
}
