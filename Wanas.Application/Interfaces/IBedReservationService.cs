using Wanas.Application.DTOs.Booking;

namespace Wanas.Application.Interfaces
{
    public interface IBedReservationService
    {
        Task<ReserveBedsResponseDto> ReserveBedsAsync(string userId, ReserveBedsRequestDto request);
        Task<bool> ConfirmReservationAsync(string ownerId, ConfirmReservationRequestDto request);
        Task<bool> CancelReservationAsync(string userId, int reservationId);
        Task<ReserveBedsResponseDto?> GetReservationAsync(int reservationId);
        Task<int> ExpireOldReservationsAsync(TimeSpan olderThan); // maintenance
    }
}
