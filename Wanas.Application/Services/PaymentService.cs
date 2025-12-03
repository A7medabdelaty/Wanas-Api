using Wanas.Application.DTOs.Booking;
using Wanas.Application.Interfaces;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;

        public PaymentService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PaymentResultDto> PayDepositAsync(string userId, PayDepositRequestDto dto)
        {
            var reservation = await _uow.Reservations.GetByIdAsync(dto.ReservationId);

            if (reservation == null)
                return new PaymentResultDto(false, "Reservation not found.");

            if (reservation.UserId != userId)
                return new PaymentResultDto(false, "You do not own this reservation.");

            // simulate success
            reservation.Status = ReservationStatus.DepositPaid;
            reservation.DepositAmount = dto.DepositAmount;
            reservation.PaidAt = DateTime.UtcNow;

            await _uow.CommitAsync();

            return new PaymentResultDto(true, "Mock payment successful.");
        }
    }

}
