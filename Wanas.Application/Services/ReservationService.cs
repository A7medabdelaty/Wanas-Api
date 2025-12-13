using AutoMapper;
using Wanas.Application.DTOs.Payment;
using Wanas.Application.DTOs.Reservation;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Wanas.Application.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IRealTimeNotifier _notifier;
        private readonly IChromaIndexingService _chromaIndexingService;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IUnitOfWork uow, 
            IMapper mapper, 
            IRealTimeNotifier notifier,
            IChromaIndexingService chromaIndexingService,
            ILogger<ReservationService> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _notifier = notifier;
            _chromaIndexingService = chromaIndexingService;
            _logger = logger;
        }

        public async Task<ReservationDto> CreateReservationAsync(CreateReservationRequestDto request, string userId)
        {
            var listing = await _uow.Listings.GetByIdAsync(request.ListingId);
            if (listing == null)
                throw new Exception("ListingNotFound");

            DateTime from = request.StartDate.Date;
            DateTime to = from.AddDays(request.DurationInDays);

            // 1. Get beds that are truly available (not rented AND not held by pending reservations)
            var beds = await _uow.Beds.GetTemporarilyAvailableBedsAsync(
                    request.ListingId,
                    request.BedIds
                );

            if (beds.Count != request.BedIds.Count())
                throw new Exception("SomeBedsUnavailable");

            // 2. Compute price
            decimal totalPrice = beds
            .Sum(b => (b.Room.PricePerBed / 30m))
            * request.DurationInDays;

            // 3. Create reservation (pending)
            var reservation = new Reservation
            {
                ListingId = request.ListingId,
                UserId = userId,
                FromDate = from,
                ToDate = to,
                TotalPrice = totalPrice,
                PaymentStatus = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Beds = beds.Select(b => new BedReservation { BedId = b.Id }).ToList()
            };

            await _uow.Reservations.AddAsync(reservation);

            foreach (var bed in beds)
                bed.IsAvailable = false;

            await _uow.CommitAsync();

            return _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<List<ReservationListItemDto>> GetRenterReservationsAsync(string userId)
        {
            var reservations = await _uow.Reservations.GetByRenterAsync(userId);

            return _mapper.Map<List<ReservationListItemDto>>(reservations);
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

            if (reservation.CreatedAt < DateTime.UtcNow.AddMinutes(-30))
                throw new Exception("ReservationExpired");

            // ---------- CHECK IF BEDS ARE STILL AVAILABLE ----------
            var beds = await _uow.Beds.GetByReservationIdAsync(reservationId);

            foreach (var b in beds)
            {
                // If another user took the bed OR there is a conflicting pending reservation
                bool unavailable =
                    b.RenterId != null ||
                    b.BedReservations.Any(br =>
                        br.ReservationId != reservationId &&
                        br.Reservation.PaymentStatus == PaymentStatus.Pending &&
                        br.Reservation.CreatedAt > DateTime.UtcNow.AddMinutes(-30));

                if (unavailable)
                    throw new Exception("SomeBedsAreNoLongerAvailable");
            }

            var listing = await _uow.Listings.GetByIdAsync(reservation.ListingId);
            if (listing == null)
                throw new Exception("ListingNotFound");

            decimal depositAmount = Math.Round(reservation.TotalPrice * 0.20m, 2);
            decimal remainingAmount = reservation.TotalPrice - depositAmount;

            bool paymentSuccess = true;
            if (!paymentSuccess)
                throw new Exception("PaymentFailed");

            foreach (var b in beds)
                b.RenterId = userId;

            reservation.PaymentStatus = PaymentStatus.Sucess;
            reservation.DepositAmount = depositAmount;
            reservation.RemainingAmount = remainingAmount;
            reservation.PaymentMethod = payment.PaymentMethod;
            reservation.PaidAt = DateTime.UtcNow;

            await _uow.CommitAsync();

            await _notifier.NotifyOwnerAsync(
                listing.UserId,
                $"Deposit paid for reservation #{reservation.Id} in listing {reservation.ListingId}"
            );

            // Check if all beds are occupied (Inactive Logic)
            var allBeds = await _uow.Beds.GetBedsByListingIdAsync(reservation.ListingId);
            if (allBeds.All(b => b.RenterId != null))
            {
                listing.IsActive = false;
                _uow.Listings.Update(listing);
                await _uow.CommitAsync();

                // Remove from ChromaDB index
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _chromaIndexingService.RemoveListingFromIndexAsync(listing.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update ChromaDB index for full listing {ListingId}", listing.Id);
                    }
                });
            }

            return _mapper.Map<ReservationDto>(reservation);
        }
    }

}
