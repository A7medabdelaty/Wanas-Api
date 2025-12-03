using AutoMapper;
using Wanas.Application.DTOs.Payment;
using Wanas.Application.DTOs.Reservation;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IRealTimeNotifier _notifier;

        public ReservationService(IUnitOfWork uow, IMapper mapper, IRealTimeNotifier notifier)
        {
            _uow = uow;
            _mapper = mapper;
            _notifier = notifier;
        }

        public async Task<ReservationDto> CreateReservationAsync(CreateReservationRequestDto request, string userId)
        {
            var listing = await _uow.Listings.GetByIdAsync(request.ListingId);
            if (listing == null)
                throw new Exception("ListingNotFound");

            var beds = await _uow.Beds.GetByIdsAsync(request.BedIds);

            beds = beds.Where(b =>
                b.Room.ApartmentListing.ListingId == request.ListingId &&
                b.RenterId == null
            ).ToList();

            if (beds.Count != request.BedIds.Count)
                throw new Exception("SomeBedsUnavailable");

            // Price per room
            decimal total = beds.Count * beds.First().Room.PricePerBed;

            var reservation = new Reservation
            {
                ListingId = request.ListingId,
                UserId = userId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalPrice = total,
                DepositAmount = total * 0.20m,
                PaymentStatus = PaymentStatus.Pending,
                Beds = beds.Select(b => new BedReservation { BedId = b.Id }).ToList()
            };

            await _uow.Reservations.AddAsync(reservation);
            await _uow.CommitAsync();

            return _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<ReservationDto> ConfirmDepositAsync(int reservationId, string userId)
        {
            var res = await _uow.Reservations.GetFullReservationAsync(reservationId);
            if (res == null)
                throw new Exception("ReservationNotFound");

            // Mock payment
            var payment = new MockPaymentResultDto
            {
                Success = true,
                TransactionId = Guid.NewGuid().ToString()
            };

            if (!payment.Success)
            {
                res.PaymentStatus = PaymentStatus.Failed;
                await _uow.CommitAsync();
                throw new Exception("PaymentFailed");
            }

            res.PaymentStatus = PaymentStatus.Sucess;

            // assign beds to user
            foreach (var bedRes in res.Beds)
            {
                var bed = await _uow.Beds.GetByIdAsync(bedRes.BedId);
                bed.RenterId = userId;
            }

            await _uow.CommitAsync();

            // notify owner
            await _notifier.NotifyOwnerAsync(res.Listing.UserId,
                $"New reservation deposit paid for listing {res.ListingId}");

            return _mapper.Map<ReservationDto>(res);
        }

        public async Task<List<ReservationDto>> GetOwnerReservationsAsync(string ownerId)
        {
            var reservations = await _uow.Reservations.GetReservationsByOwnerAsync(ownerId);
            return _mapper.Map<List<ReservationDto>>(reservations);
        }

        public async Task<ReservationDto> GetReservationAsync(int id)
        {
            var reservation = await _uow.Reservations.GetFullReservationAsync(id);
            return _mapper.Map<ReservationDto>(reservation);
        }
        public async Task<ReservationDto> PayDepositAsync(
            int reservationId,
            DepositPaymentRequestDto payment,
            string userId)
        {
            var reservation = await _uow.Reservations.GetReservationWithBedsAsync(reservationId);

            if (reservation == null)
                throw new Exception("ReservationNotFound");

            if (reservation.UserId != userId)
                throw new Exception("Forbidden");

            if (reservation.PaymentStatus != PaymentStatus.Pending)
                throw new Exception("AlreadyPaidOrInvalidState");

            // Load listing owner
            var listing = await _uow.Listings.GetByIdAsync(reservation.ListingId);
            if (listing == null)
                throw new Exception("ListingNotFound");

            decimal depositAmount = reservation.TotalPrice * 0.20m;

            // ------- MOCK PAYMENT -------
            bool paymentSuccess = true; // For now always success
            if (!paymentSuccess)
                throw new Exception("PaymentFailed");

            // Assign beds to user
            var beds = await _uow.Beds.GetByReservationIdAsync(reservationId);
            foreach (var b in beds)
                b.RenterId = userId;

            reservation.PaymentStatus = PaymentStatus.Sucess;
            reservation.DepositAmount = depositAmount;
            reservation.PaymentMethod = payment.PaymentMethod;
            reservation.PaidAt = DateTime.UtcNow;

            await _uow.CommitAsync();

            // Notify owner
            await _notifier.NotifyOwnerAsync(
                listing.UserId,
                $"New reservation deposit paid for listing {reservation.ListingId}"
            );

            return _mapper.Map<ReservationDto>(reservation);
        }

    }

}
