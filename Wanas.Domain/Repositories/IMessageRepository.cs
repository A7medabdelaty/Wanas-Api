using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
    public interface IMessageRepository : IGenericRepository<Message>
    {
        Task<IEnumerable<Message>> GetMessagesByChatIdAsync(int chatId, int limit = 50);
        Task<List<Message>> GetMessagesWithReadReceiptsAsync(int chatId, string userId);

    }
}
