namespace Wanas.Application.DTOs.Booking
{
    public class PaymentResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal? PaidAmount { get; set; }
        public DateTime? PaidAt { get; set; }

        public PaymentResultDto() { }

        public PaymentResultDto(bool success, string message, decimal? paidAmount = null)
        {
            Success = success;
            Message = message;
            PaidAmount = paidAmount;
            PaidAt = success ? DateTime.UtcNow : null;
        }
    }
}
