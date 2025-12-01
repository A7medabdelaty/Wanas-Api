using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class BookingApprovalRepository : GenericRepository<BookingApproval>, IBookingApprovalRepository
    {
        public BookingApprovalRepository(AppDBContext context) : base(context) { }

        public async Task<bool> IsApprovedAsync(int listingId, string userId)
        {
            return await _context.BookingApprovals
                .AnyAsync(x => x.ListingId == listingId && x.UserId == userId);
        }
    }
}
