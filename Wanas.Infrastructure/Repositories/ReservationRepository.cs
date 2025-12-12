using Microsoft.EntityFrameworkCore;
using System;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
    {
        private readonly AppDBContext _context;

        public ReservationRepository(AppDBContext ctx) : base(ctx)
        {
            _context = ctx;
        }

        public async Task<Reservation> GetFullReservationAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.Beds)
                .Include(r => r.Listing)
                .ThenInclude(l => l.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Reservation>> GetReservationsByOwnerAsync(string ownerId)
        {
            var expiryLimit = DateTime.UtcNow.AddMinutes(-30);

            return await _context.Reservations
                .Include(r => r.Beds)
                    .ThenInclude(br => br.Bed)
                        .ThenInclude(b => b.Room)
                .Include(r => r.Listing)
                .Where(r =>
                    r.Listing.UserId == ownerId && (
                        r.PaymentStatus == PaymentStatus.Sucess ||
                        (r.PaymentStatus == PaymentStatus.Pending && r.CreatedAt >= expiryLimit)
                    )
                )
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Reservation?> GetReservationWithBedsAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.Beds)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Reservation>> GetByOwnerAsync(string ownerId)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Beds)
                .Include(r => r.Listing)
                .Where(r => r.Listing.UserId == ownerId)
                .ToListAsync();
        }
        public async Task<List<Reservation>> GetByRenterAsync(string renterId)
        {
            return await _context.Reservations
                .Include(r => r.Beds)
                    .ThenInclude(br => br.Bed)
                        .ThenInclude(b => b.Room)
                .Include(r => r.Listing)
                    .ThenInclude(l => l.ListingPhotos)
                .Include(r => r.Listing)
                    .ThenInclude(l => l.ApartmentListing)
                .Where(r => r.UserId == renterId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }

}
