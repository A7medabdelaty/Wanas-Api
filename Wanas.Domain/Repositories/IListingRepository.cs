using Wanas.Domain.Entities;
using Wanas.Domain.Models;

namespace Wanas.Domain.Repositories
{
    public interface IListingRepository:IGenericRepository<Listing>
    {
        Task<List<Listing>> GetActiveListingsAsync();
        Task<(IEnumerable<Listing> Listings, int TotalCount)> SearchListingsAsync(ListingSearchFilters filters);

    }
}
