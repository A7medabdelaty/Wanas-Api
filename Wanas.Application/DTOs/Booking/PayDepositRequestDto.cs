namespace Wanas.Application.DTOs.Booking
{
    public class PayDepositRequestDto
    {
        public int ReservationId { get; set; }
        public decimal DepositAmount { get; set; }
    }
}
