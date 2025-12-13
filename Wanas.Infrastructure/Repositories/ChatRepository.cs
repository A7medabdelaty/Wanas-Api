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
                    .ThenInclude(cp => cp.User)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .Where(c => c.ChatParticipants.Any(p => p.UserId == userId))
                .ToListAsync();
        }

        public async Task<Chat?> GetPrivateChatBetweenAsync(string userA, string userB)
        {
            return await _context.Chats
                .Include(c => c.ChatParticipants).ThenInclude(cp => cp.User)
                .Include(c => c.Messages)
                .Where(c => !c.IsGroup)
                .Where(c => c.ListingId == null)
                .Where(c =>
                    c.ChatParticipants.Any(p => p.UserId == userA) &&
                    c.ChatParticipants.Any(p => p.UserId == userB)
                )
                .FirstOrDefaultAsync();
        }

        public async Task<Chat?> GetChatWithMessagesAsync(int chatId)
        {
            return await _context.Chats
                .Include(c => c.ChatParticipants).ThenInclude(cp => cp.User)
                .Include(c => c.Messages).ThenInclude(m => m.ReadReceipts)
                .FirstOrDefaultAsync(c => c.Id == chatId);
        }

        public async Task<Chat> GetChatWithUsersAsync(int chatId)
        {
            return await _context.Chats
                .Include(c => c.ChatParticipants).ThenInclude(cp => cp.User)
                .FirstOrDefaultAsync(c => c.Id == chatId);
        }

        public async Task<Chat?> GetChatWithParticipantsAsync(int chatId)
        {
            return await _context.Chats
                .Include(c => c.ChatParticipants).ThenInclude(cp => cp.User)
                .FirstOrDefaultAsync(c => c.Id == chatId);
        }

        public async Task<Chat?> GetPrivateChatForListingAsync(string ownerId, string userId, int listingId)
        {
            return await _context.Chats
                .Include(c => c.ChatParticipants).ThenInclude(cp => cp.User)
                .Include(c => c.Messages)
                .Where(c => !c.IsGroup && c.ListingId == listingId)
                .Where(c =>
                    c.ChatParticipants.Any(p => p.UserId == ownerId) &&
                    c.ChatParticipants.Any(p => p.UserId == userId)
                )
                .FirstOrDefaultAsync();
        }

    }

}
