using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class AppealRepository : GenericRepository<Appeal>, IAppealRepository
    {
        public AppealRepository(AppDBContext context) : base(context)
        {
        }

        // Implement custom queries here if needed
        // public async Task<IEnumerable<Appeal>> GetPendingAppealsAsync()
        //     => await FindAsync(a => a.Status == AppealStatus.Pending);
    }
}
