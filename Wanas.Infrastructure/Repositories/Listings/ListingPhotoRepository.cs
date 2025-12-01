using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories.Listings;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories.Listings
{
    public class ListingPhotoRepository : GenericRepository<ListingPhoto>, IListingPhotoRepository
    {
        public ListingPhotoRepository(AppDBContext context) : base(context)
        {

        }

        public async Task<IEnumerable<ListingPhoto>> GetPhotosByListingIdAsync(int listingId)
        {
            return await _context.ListingPhotos
                .Where(x => x.ListingId == listingId)
                .ToListAsync();
        }

        public async Task<ListingPhoto> GetPhotoWithListingByIdAsync(int photoId)
        {
            return await _context.ListingPhotos
                .Include(p => p.Listing)
                .FirstOrDefaultAsync(p => p.Id == photoId);
        }

    }
}
