using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
    public interface IAppealRepository : IGenericRepository<Appeal>
    {
        // Add custom queries if needed
        // Task<IEnumerable<Appeal>> GetPendingAppealsAsync();
        // Task<IEnumerable<Appeal>> GetAppealsByUserIdAsync(string userId);
    }
}
