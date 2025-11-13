using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class ChatParticipantRepository : GenericRepository<ChatParticipant>, IChatParticipantRepository
    {
        public ChatParticipantRepository(AppDBContext context) : base(context) { }

        public async Task<ChatParticipant?> GetParticipantAsync(int chatId, string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.ChatId == chatId && p.UserId == userId);
        }
    }
}
