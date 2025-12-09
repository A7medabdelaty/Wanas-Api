using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories.Listings
{
    public interface IListingRepository : IGenericRepository<Listing>
    {
        Task<IEnumerable<Listing>> GetAllListingsAsync();
        Task<Listing?> GetListingWithDetailsAsync(int id);
        Task<Listing?> GetListingWithDetailsTrackedAsync(int id);
        Task<IEnumerable<Listing>> GetListingsByUserAsync(string userId);
        Task<IEnumerable<Listing>> GetListingsByCityAsync(string city);
        Task<IEnumerable<Listing>> GetActiveListingsAsync();
        Task<IEnumerable<Listing>> SearchByTitleAsync(string keyword);
        IQueryable<Listing> GetQueryableWithIncludes();

            Task<IEnumerable<Listing>> GetPendingWithOwnerAsync();
        
        IQueryable<Listing> ApplyKeywordSearch(IQueryable<Listing> query, string keyword);
    }
}
