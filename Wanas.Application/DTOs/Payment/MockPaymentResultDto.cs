namespace Wanas.Application.DTOs.Payment
{
    public class MockPaymentResultDto
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    }

}
