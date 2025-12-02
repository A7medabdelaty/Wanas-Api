using Wanas.Application.DTOs.Booking;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class BedReservationService : IBedReservationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRealTimeNotifier _notifier;
        private readonly TimeSpan _reservationTtl = TimeSpan.FromMinutes(60);

        public BedReservationService(IUnitOfWork uow, IRealTimeNotifier notifier)
        {
            _uow = uow;
            _notifier = notifier;
        }

        // Main reservation endpoint
        public async Task<ReserveBedsResponseDto> ReserveBedsAsync(string userId, ReserveBedsRequestDto request)
        {
            if (request.BedIds == null || !request.BedIds.Any())
                throw new ArgumentException("No beds provided.");

            // validate beds
            var beds = await _uow.Beds.FindAsync(
                b => request.BedIds.Contains(b.Id)
                     && b.Room.ApartmentListing.ListingId == request.ListingId
            );

            var bedList = beds.ToList();
            if (bedList.Count != request.BedIds.Count)
                throw new InvalidOperationException("Invalid beds for listing.");

            // beds already taken?
            if (bedList.Any(b => b.RenterId != null))
                throw new InvalidOperationException("Some beds already have renter.");

            // active reservations conflict?
            var conflicts = await _uow.BedReservations.GetActiveReservationsForBedsAsync(request.BedIds);
            if (conflicts.Any())
                throw new InvalidOperationException("Beds already temporarily reserved.");

            using var tx = await _uow.BeginTransactionAsync();

            var reservation = new BedReservation
            {
                ListingId = request.ListingId,
                UserId = userId,
                ReservedAt = DateTime.UtcNow,
                Items = request.BedIds.Select(id => new BedReservationItem { BedId = id }).ToList()
            };

            await _uow.BedReservations.AddAsync(reservation);
            await _uow.CommitAsync();

            await tx.CommitAsync();

            return new ReserveBedsResponseDto
            {
                ReservationId = reservation.Id,
                ListingId = reservation.ListingId,
                ReservedAt = reservation.ReservedAt,
                ReservedBedIds = reservation.Items.Select(i => i.BedId),
                IsConfirmed = false
            };
        }


        // Confirm by owner (owner approves payment)
        public async Task<bool> ConfirmReservationAsync(string ownerId, ConfirmReservationRequestDto request)
        {
            var reservation = await _uow.BedReservations.GetByIdWithItemsAsync(request.ReservationId);
            if (reservation == null || reservation.IsCancelled || reservation.IsConfirmed)
                return false;

            var listing = await _uow.Listings.GetByIdAsync(reservation.ListingId);
            if (listing == null || listing.UserId != ownerId)
                return false;

            var bedIds = reservation.Items.Select(i => i.BedId).ToList();
            var beds = (await _uow.Beds.FindAsync(b => bedIds.Contains(b.Id))).ToList();

            if (beds.Any(b => b.RenterId != null))
                return false;

            using var tx = await _uow.BeginTransactionAsync();

            foreach (var bed in beds)
            {
                bed.RenterId = reservation.UserId;
                _uow.Beds.Update(bed);
            }

            reservation.IsConfirmed = true;
            reservation.ConfirmedAt = DateTime.UtcNow;
            _uow.BedReservations.Update(reservation);

            await _uow.CommitAsync();
            await tx.CommitAsync();

            return true;
        }


        public async Task<bool> CancelReservationAsync(string userId, int reservationId)
        {
            var reservation = await _uow.BedReservations.GetByIdWithItemsAsync(reservationId);
            if (reservation == null)
                return false;
            if (reservation.UserId != userId)
                return false; // only requester can cancel before confirm
            if (reservation.IsConfirmed || reservation.IsCancelled)
                return false;

            reservation.IsCancelled = true;
            reservation.CancelledAt = DateTime.UtcNow;
            _uow.BedReservations.Update(reservation);
            await _uow.CommitAsync();
            return true;
        }

        public async Task<ReserveBedsResponseDto?> GetReservationAsync(int reservationId)
        {
            var r = await _uow.BedReservations.GetByIdWithItemsAsync(reservationId);
            if (r == null)
                return null;

            return new ReserveBedsResponseDto
            {
                ReservationId = r.Id,
                ListingId = r.ListingId,
                ReservedBedIds = r.Items.Select(i => i.BedId),
                ReservedAt = r.ReservedAt,
                IsConfirmed = r.IsConfirmed
            };
        }

        // maintenance: expire old pending reservations
        public async Task<int> ExpireOldReservationsAsync(TimeSpan olderThan)
        {
            var cutoff = DateTime.UtcNow.Subtract(olderThan);
            var pending = await _uow.BedReservations
                .FindAsync(r => !r.IsConfirmed && !r.IsCancelled && r.ReservedAt < cutoff);

            var list = pending.ToList();
            foreach (var r in list)
            {
                r.IsCancelled = true;
                r.CancelledAt = DateTime.UtcNow;
                _uow.BedReservations.Update(r);
            }

            await _uow.CommitAsync();
            return list.Count;
        }
    }
}
