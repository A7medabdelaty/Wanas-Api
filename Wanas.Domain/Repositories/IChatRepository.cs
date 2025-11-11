using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
    public interface IChatRepository : IGenericRepository<Chat>
    {
        Task<IEnumerable<Chat>> GetUserChatsAsync(string userId);
        Task<Chat?> GetChatWithMessagesAsync(int chatId);
        Task<Chat?> GetChatWithParticipantsAsync(int chatId);
    }
}
