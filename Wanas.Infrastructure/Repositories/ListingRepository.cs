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

        public IQueryable<Listing> GetQueryableWithIncludes()
        {
            return _context.Listings
                .AsNoTracking()
                .Include(x => x.ApartmentListing)
                .Include(x => x.ListingPhotos)
                .Include(x => x.User)
                .Include(x => x.Comments);
        }
        public IQueryable<Listing> ApplyKeywordSearch(IQueryable<Listing> query, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return query;

            keyword = keyword.ToLower().Trim();

            return query.Where(x =>
                x.Title.ToLower().Contains(keyword) ||
                x.Description.ToLower().Contains(keyword) ||
                x.City.ToLower().Contains(keyword)
            );
        }
    }
}
