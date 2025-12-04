using Wanas.Application.DTOs.Booking;
using Wanas.Application.DTOs.Payment;
using Wanas.Domain.Entities;

namespace Wanas.Application.Interfaces
{
    public interface IBedReservationService
    {
        Task<Reservation> ReserveBedsAsync(string userId, ReserveBedsRequestDto dto);
        Task<bool> ApprovePaymentAsync(int reservationId, string ownerId);
        Task<MockPaymentResultDto> PayDepositAsync(int reservationId, string userId);
        Task<bool> ConfirmReservationAsync(string ownerId, ConfirmReservationRequestDto dto);
        Task<bool> CancelReservationAsync(string userId, int reservationId);
    }
}
