using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class BedRepository : GenericRepository<Bed>, IBedRepository
    {
        private readonly AppDBContext _context;

        public BedRepository(AppDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Bed>> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.Beds
                .Where(x => ids.Contains(x.Id))
                .Include(x => x.Room)
                .ToListAsync();
        }

        public async Task<List<Bed>> GetBedsByRoomIdAsync(int roomId)
        {
            return await _context.Beds
                .Where(x => x.RoomId == roomId)
                .ToListAsync();
        }

        public async Task<List<Bed>> GetBedsByListingIdAsync(int listingId)
        {
            return await _context.Beds
                .Include(b => b.Room)
                .ThenInclude(r => r.ApartmentListing)
                .Where(b => b.Room.ApartmentListing.ListingId == listingId)
                .ToListAsync();
        }

        public async Task<List<Bed>> GetAvailableBedsByRoomAsync(int roomId)
        {
            return await _context.Beds
                .Where(x => x.RoomId == roomId && x.RenterId == null)
                .ToListAsync();
        }

        public async Task<List<Bed>> GetAvailableBedsByListingAsync(int listingId)
        {
            return await _context.Beds
                .Include(x => x.Room)
                .ThenInclude(a=>a.ApartmentListing)
                .Where(x => x.Room.ApartmentListing.ListingId == listingId && x.RenterId == null)
                .ToListAsync();
        }
    }
}
