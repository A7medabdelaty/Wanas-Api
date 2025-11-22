using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
 public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
 {
 public PaymentRepository(AppDBContext context) : base(context) { }
 public async Task<IEnumerable<Payment>> GetByListingAsync(int listingId) => await _context.Payments.Where(p => p.ListingId == listingId).ToListAsync();
 public async Task<IEnumerable<Payment>> GetByUserAsync(string userId) => await _context.Payments.Where(p => p.UserId == userId).ToListAsync();
 }
}