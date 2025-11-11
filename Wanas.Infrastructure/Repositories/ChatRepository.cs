using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class ChatRepository : GenericRepository<Chat>, IChatRepository
    {
        public ChatRepository(AppDBContext context) : base(context) { }

        public async Task<IEnumerable<Chat>> GetUserChatsAsync(string userId)
        {
            return await _context.Chats
                .Include(c => c.ChatParticipants)
                .Include(c => c.Messages)
                .Where(c => c.ChatParticipants.Any(p => p.UserId == userId))
                .ToListAsync();
        }

        public async Task<Chat?> GetChatWithMessagesAsync(int chatId)
        {
            return await _context.Chats
                .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
                .Include(c => c.ChatParticipants)
                .FirstOrDefaultAsync(c => c.Id == chatId);
        }
    }
}
