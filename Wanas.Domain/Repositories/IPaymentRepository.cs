using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
 public interface IPaymentRepository : IGenericRepository<Payment>
 {
 Task<IEnumerable<Payment>> GetByListingAsync(int listingId);
 Task<IEnumerable<Payment>> GetByUserAsync(string userId);
 }
}