using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
 public interface IPayoutRepository : IGenericRepository<Payout>
 {
 Task<IEnumerable<Payout>> GetPendingAsync();
 }
}