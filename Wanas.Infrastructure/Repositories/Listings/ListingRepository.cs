using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories.Listings;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories.Listings
{
    public class ListingRepository : GenericRepository<Listing>, IListingRepository
    {
        public ListingRepository(AppDBContext context) : base(context)
        {
            
        }

        public async Task<IEnumerable<Listing>> GetActiveListingsAsync()
        {
            return await _context.Listings
                .Where(l => l.IsActive)
                .Include(l => l.User)
                .ThenInclude(u => u.UserPreference)
                .Include(l => l.ApartmentListing)
                .Include(l => l.ListingPhotos)
                .ToListAsync();
        }

        public async Task<IEnumerable<Listing>> GetListingsByCityAsync(string city)
        {
            return await _context.Listings
                .Where(l => l.City.ToLower() == city.ToLower())
                .ToListAsync();
        }

        public async Task<IEnumerable<Listing>> GetListingsByUserAsync(string userId)
        {
            return await _context.Listings
                .Where(l => l.UserId == userId)
                .ToListAsync();
        }

        public async Task<Listing> GetListingWithDetailsAsync(int id)
        {
            return await _context.Listings
                .Include(l => l.User)
            .Include(l => l.ListingPhotos)
            .Include(l => l.ApartmentListing)
                .ThenInclude(a => a.Rooms)
                    .ThenInclude(r => r.Beds)
            .Include(l => l.Comments)
            .Include(l => l.Matches)
            .Include(l => l.Payments)
            .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Listing>> SearchByTitleAsync(string keyword)
        {
            return await _context.Listings
                .Where(l => l.Title.ToLower().Contains(keyword.ToLower()))
                .ToListAsync();
        }

        public async Task<IEnumerable<Listing>> GetAllListingsAsync()
        {
            return await _context.Listings
                .Include(l => l.ListingPhotos)
                .Include(l => l.User)
                .Include(l => l.ApartmentListing)
                   .ThenInclude(al => al.Rooms)
                       .ThenInclude(r => r.Beds)
                .OrderByDescending(l => l.CreatedAt)
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
