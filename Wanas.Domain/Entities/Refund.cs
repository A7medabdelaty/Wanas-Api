namespace Wanas.Domain.Entities
{
    public class Refund
    {
        public int RefundId { get; set; }
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Processed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public Payment Payment { get; set; }
    }
}