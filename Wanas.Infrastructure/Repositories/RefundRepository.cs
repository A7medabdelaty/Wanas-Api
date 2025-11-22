using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
 public class RefundRepository : GenericRepository<Refund>, IRefundRepository
 {
 public RefundRepository(AppDBContext context) : base(context) { }
 }
}