using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories.Listings
{
    public interface IRoomRepository : IGenericRepository<Room>
    {
        Task<IEnumerable<Room>> GetRoomsByApartmentIdAsync(int apartmentId);
        Task<Room?> GetRoomWithBedsAsync(int roomId);
    }
}
