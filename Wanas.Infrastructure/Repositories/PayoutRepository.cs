using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
 public class PayoutRepository : GenericRepository<Payout>, IPayoutRepository
 {
 public PayoutRepository(AppDBContext context) : base(context) { }
 public async Task<IEnumerable<Payout>> GetPendingAsync() => await _context.Payouts.Where(p => p.Status == "Pending").ToListAsync();
 }
}