namespace Wanas.Domain.Entities
{
    public class Payout
    {
        public int PayoutId { get; set; }
        public string HostUserId { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal CommissionTotal { get; set; }
        public decimal NetAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public string? Notes { get; set; }
        public ApplicationUser HostUser { get; set; }
    }
}