using Microsoft.EntityFrameworkCore;
using System;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
    {
        public ReservationRepository(AppDBContext context) : base(context)
        {

        }
        public async Task<Reservation?> GetFullReservationAsync(int reservationId)
        {
            return await _context.Reservations
                .Include(r => r.ReservedBeds)
                    .ThenInclude(b => b.Room)
                        .ThenInclude(rm => rm.ApartmentListing)
                            .ThenInclude(al => al.Listing)
                .FirstOrDefaultAsync(r => r.Id == reservationId);
        }
    }
}
