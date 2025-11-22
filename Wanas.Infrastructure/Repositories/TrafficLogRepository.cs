using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class TrafficLogRepository : GenericRepository<TrafficLog>, ITrafficLogRepository
    {
        public TrafficLogRepository(AppDBContext context) : base(context) { }
    }
}