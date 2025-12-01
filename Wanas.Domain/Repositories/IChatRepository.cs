using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
    public interface IChatRepository : IGenericRepository<Chat>
    {
        Task<Chat?> GetPrivateChatBetweenAsync(string userA, string userB);
        Task<IEnumerable<Chat>> GetUserChatsAsync(string userId);
        Task<Chat?> GetChatWithMessagesAsync(int chatId);
        Task<Chat> GetChatWithUsersAsync(int chatId);
        Task<Chat?> GetChatWithParticipantsAsync(int chatId);
    }
}
