using Wanas.Application.DTOs.Payment;
using Wanas.Application.DTOs.Reservation;

namespace Wanas.Application.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationDto> CreateReservationAsync(CreateReservationRequestDto request, string userId);
        Task<ReservationDto> ConfirmDepositAsync(int reservationId, string userId);
        Task<List<ReservationDto>> GetOwnerReservationsAsync(string ownerId);
        Task<ReservationDto> GetReservationAsync(int id);
        Task<ReservationDto> PayDepositAsync(int reservationId, DepositPaymentRequestDto payment, string userId);
    }
}
