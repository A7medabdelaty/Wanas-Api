using System.Collections.Generic;
using System.Threading.Tasks;
using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
    public interface IMessageRepository : IGenericRepository<Message>
    {
        Task<IEnumerable<Message>> GetMessagesByChatIdAsync(int chatId, int limit = 50);
        Task<IEnumerable<Message>> GetMessagesWithReadReceiptsAsync(int chatId, string userId);
        Task<Dictionary<int, int>> GetUnreadMessageCountsAsync(string userId, IEnumerable<int> chatIds);

    }
}
