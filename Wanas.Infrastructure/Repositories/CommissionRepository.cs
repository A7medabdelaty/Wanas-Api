using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
 public class CommissionRepository : GenericRepository<Commission>, ICommissionRepository
 {
 public CommissionRepository(AppDBContext context) : base(context) { }
 }
}