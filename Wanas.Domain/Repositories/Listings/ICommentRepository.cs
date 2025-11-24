using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories.Listings
{
    public interface ICommentRepository : IGenericRepository<Comment>
    {
        Task<IEnumerable<Comment>> GetCommentsByListingAsync(int listingId);
        Task<IEnumerable<Comment>> GetCommentsByUserAsync(string userId);
        Task<Comment?> GetCommentWithAuthorAndRepliesAsync(int id);
    }
}
