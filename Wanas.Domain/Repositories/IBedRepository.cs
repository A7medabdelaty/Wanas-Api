using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
    public interface IBedRepository : IGenericRepository<Bed>
    {
        Task<List<Bed>> GetByIdsAsync(IEnumerable<int> ids);
        Task<List<Bed>> GetBedsByRoomIdAsync(int roomId);
        Task<List<Bed>> GetBedsByListingIdAsync(int listingId);
        Task<List<Bed>> GetByReservationIdAsync(int reservationId);
        Task<List<Bed>> GetAvailableBedsByRoomAsync(int roomId);
        Task<List<Bed>> GetAvailableBedsByListingAsync(int listingId);
    }
}
