using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class ListingRepository : GenericRepository<Listing>, IListingRepository
    {
        public ListingRepository(AppDBContext context) : base(context)
        {
        }

        public async Task<List<Listing>> GetActiveListingsAsync()
        {
            return await _context.Listings
                .Include(l => l.User)
                .ThenInclude(u => u.UserPreference)
                .Include(l => l.ApartmentListing)
                .Where(l => l.IsActive && !l.User.IsDeleted)
                .ToListAsync();
        }
    }
}
