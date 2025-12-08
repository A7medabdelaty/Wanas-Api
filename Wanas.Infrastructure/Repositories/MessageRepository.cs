using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<IEnumerable<Message>> GetMessagesWithReadReceiptsAsync(int chatId, string userId)
        {
            return await _context.Messages
                .Include(m => m.ReadReceipts)
                .Where(m => m.ChatId == chatId && m.SenderId != userId) // Only messages sent by OTHERS
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetUnreadMessageCountsAsync(string userId, IEnumerable<int> chatIds)
        {
            var counts = await _context.Messages
                .Where(m => chatIds.Contains(m.ChatId) && 
                            m.SenderId != userId && 
                            !m.ReadReceipts.Any(r => r.UserId == userId))
                .GroupBy(m => m.ChatId)
                .Select(g => new { ChatId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ChatId, x => x.Count);

            return counts;
        }
    }
}
