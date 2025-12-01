using Wanas.Domain.Entities;
using Wanas.Domain.Enums;

namespace Wanas.Domain.Repositories
{
    public interface IBookingApprovalRepository : IGenericRepository<BookingApproval>
    {
        Task<bool> IsApprovedAsync(int listingId, string userId);
        Task<bool> ExistsAsync(int listingId, string userId, ApprovalType type);
    }
}
