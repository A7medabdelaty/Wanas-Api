using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories.Listings
{
    public interface IListingPhotoRepository : IGenericRepository<ListingPhoto>
    {
        Task<IEnumerable<ListingPhoto>> GetPhotosByListingIdAsync(int listingId);
    }
}
