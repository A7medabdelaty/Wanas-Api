using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        public MessageRepository(AppDBContext context) : base(context) { }

        public async Task<IEnumerable<Message>> GetMessagesByChatIdAsync(int chatId, int limit = 50)
        {
            return await _context.Messages
                .Where(m => m.ChatId == chatId)
                .OrderByDescending(m => m.SentAt)
                .Take(limit)
                .Include(m => m.Sender)
                .ToListAsync();
        }
    }
}
