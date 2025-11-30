using Wanas.Application.DTOs.Chat;

namespace Wanas.Application.Interfaces
{
    public interface IChatService
    {
        Task<IEnumerable<ChatDto>> GetUserChatsAsync(string userId);
        Task<ChatDto> GetOrCreatePrivateChatAsync(string userId, string ownerId);
        Task<ChatDto?> GetChatDetailsAsync(int chatId, string userId);
        Task<ChatDto> CreateChatAsync(string creatorId, CreateChatRequestDto request);
        Task<IList<int>> GetUserChatIdsAsync(string userId);
        Task<bool> AddParticipantAsync(int chatId, string userId);
        Task<bool> RemoveParticipantAsync(int chatId, string userId);
        Task<ChatDto?> UpdateChatAsync(int chatId, UpdateChatRequestDto request);
        Task<bool> DeleteChatAsync(int chatId);
        Task<bool> LeaveChatAsync(int chatId, string userId);
        Task<bool> MarkChatAsReadAsync(int chatId, string userId);
        Task<int> GetUnreadMessagesCountAsync(string userId);
        Task<IEnumerable<ChatSummaryDto>> GetRecentChatsAsync(string userId);
    }
}
