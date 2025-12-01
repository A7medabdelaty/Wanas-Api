using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
    public interface IBookingApprovalRepository : IGenericRepository<BookingApproval>
    {
        Task<bool> IsApprovedAsync(int listingId, string userId);
    }
}
