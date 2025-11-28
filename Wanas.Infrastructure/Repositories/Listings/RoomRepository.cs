using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories.Listings;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories.Listings
{
    public class RoomRepository : GenericRepository<Room>, IRoomRepository
    {
        public RoomRepository(AppDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Room>> GetRoomsByApartmentIdAsync(int apartmentId)
        {
            return await _context.Rooms
                .Where(r => r.ApartmentListingId == apartmentId)
                .Include(r => r.Beds)
                .ToListAsync();
        }

        public async Task<Room> GetRoomWithBedsAsync(int roomId)
        {
            return await _context.Rooms
                .Include(r => r.Beds)
                .FirstOrDefaultAsync(r => r.Id == roomId);
        }
    }
}
