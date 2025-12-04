using Wanas.Application.DTOs.Booking;

namespace Wanas.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResultDto> PayDepositAsync(string userId, PayDepositRequestDto dto);
    }
}
