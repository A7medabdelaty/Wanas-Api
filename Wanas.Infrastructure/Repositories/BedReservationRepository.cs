using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class BedReservationRepository : GenericRepository<BedReservation>, IBedReservationRepository
    {
        public BedReservationRepository(AppDBContext context) : base(context) { }

        public async Task<BedReservation?> GetByIdWithItemsAsync(int reservationId)
        {
            return await _context.Set<BedReservation>()
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == reservationId);
        }

        public async Task<IEnumerable<BedReservation>> GetActiveReservationsForBedsAsync(IEnumerable<int> bedIds)
        {
            var ids = bedIds.ToArray();
            return await _context.Set<BedReservation>()
                .Where(r => !r.IsCancelled && !r.IsConfirmed)
                .Where(r => r.Items.Any(i => ids.Contains(i.BedId)))
                .Include(r => r.Items)
                .ToListAsync();
        }

        public async Task<BedReservation?> GetActiveReservationForListingAsync(int listingId)
        {
            return await _context.Set<BedReservation>()
                .Where(r => r.ListingId == listingId && !r.IsCancelled && !r.IsConfirmed)
                .Include(r => r.Items)
                .OrderByDescending(r => r.ReservedAt)
                .FirstOrDefaultAsync();
        }
    }
}
