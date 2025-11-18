using Wanas.Domain.Entities;
using Wanas.Domain.Models;

namespace Wanas.Domain.Repositories
{
    public interface IListingRepository:IGenericRepository<Listing>
    {
        Task<List<Listing>> GetActiveListingsAsync();
        IQueryable<Listing> GetQueryableWithIncludes();
        IQueryable<Listing> ApplyKeywordSearch(IQueryable<Listing> query, string keyword);
    }
}
