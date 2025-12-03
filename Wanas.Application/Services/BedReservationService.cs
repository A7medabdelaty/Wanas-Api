using Wanas.Application.DTOs.Booking;
using Wanas.Application.DTOs.Payment;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class BedReservationService : IBedReservationService
    {
        private readonly IUnitOfWork _uow;

        public BedReservationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // STEP 1 — User requests reservation
        public async Task<Reservation> ReserveBedsAsync(string userId, ReserveBedsRequestDto dto)
        {
            var beds = (await _uow.Beds.FindAsync(
                b => dto.BedIds.Contains(b.Id) && b.IsAvailable))
                .ToList();

            if (beds.Count != dto.BedIds.Count)
                throw new InvalidOperationException("Some beds are no longer available.");

            decimal deposit = beds.Sum(b => b.Room.PricePerBed) * 0.20m;

            var reservation = new Reservation
            {
                UserId = userId,
                ListingId = dto.ListingId,
                Status = ReservationStatus.PendingOwnerApproval,
                DepositAmount = deposit,
                ReservedBeds = beds,
                OwnerId = beds.First().Room.ApartmentListing.Listing.UserId
            };

            await _uow.Reservations.AddAsync(reservation);

            foreach (var bed in beds)
                bed.RenterId = userId;

            await _uow.CommitAsync();

            return reservation;
        }

        // STEP 2 — Owner confirms that user can pay
        public async Task<bool> ApprovePaymentAsync(int reservationId, string ownerId)
        {
            var reservation = await _uow.Reservations.GetByIdAsync(reservationId);
            if (reservation == null || reservation.OwnerId != ownerId)
                return false;

            reservation.Status = ReservationStatus.AwaitingDepositPayment;
            _uow.Reservations.Update(reservation);
            await _uow.CommitAsync();
            return true;
        }

        // STEP 3 — User pays deposit (mock)
        public async Task<MockPaymentResultDto> PayDepositAsync(int reservationId, string userId)
        {
            var reservation = await _uow.Reservations.GetByIdAsync(reservationId);
            if (reservation == null || reservation.UserId != userId)
                throw new InvalidOperationException("Invalid reservation.");

            if (reservation.Status != ReservationStatus.AwaitingDepositPayment)
                throw new InvalidOperationException("Payment not approved.");

            reservation.Status = ReservationStatus.DepositPaid;

            _uow.Reservations.Update(reservation);
            await _uow.CommitAsync();

            return new MockPaymentResultDto
            {
                Success = true,
                TransactionId = Guid.NewGuid().ToString()
            };
        }

        // STEP 4 — Owner final confirmation
        public async Task<bool> ConfirmReservationAsync(string ownerId, ConfirmReservationRequestDto dto)
        {
            var reservation = await _uow.Reservations.GetByIdAsync(dto.ReservationId);
            if (reservation == null || reservation.OwnerId != ownerId)
                return false;

            if (reservation.Status != ReservationStatus.DepositPaid)
                return false;

            reservation.Status = ReservationStatus.Confirmed;
            _uow.Reservations.Update(reservation);

            await _uow.CommitAsync();
            return true;
        }

        // Cancel
        public async Task<bool> CancelReservationAsync(string userId, int reservationId)
        {
            var reservation = await _uow.Reservations.GetByIdAsync(reservationId);
            if (reservation == null || reservation.UserId != userId)
                return false;

            reservation.Status = ReservationStatus.Cancelled;
            _uow.Reservations.Update(reservation);

            foreach (var bed in reservation.ReservedBeds)
                bed.RenterId = null;

            await _uow.CommitAsync();
            return true;
        }
    }

}
