using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class DailyMetricsRepository : GenericRepository<DailyMetrics>, IDailyMetricsRepository
    {
        public DailyMetricsRepository(AppDBContext context) : base(context) { }
    }
}