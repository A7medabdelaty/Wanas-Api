using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
    public interface IChatParticipantRepository : IGenericRepository<ChatParticipant>
    {
        Task<ChatParticipant?> GetParticipantAsync(int chatId, string userId);
    }
}
