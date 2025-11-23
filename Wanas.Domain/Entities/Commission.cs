namespace Wanas.Domain.Entities
{
    public class Commission
    {
        public int CommissionId { get; set; }
        public int PaymentId { get; set; }
        public decimal PlatformPercent { get; set; }
        public decimal PlatformAmount { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
        public Payment Payment { get; set; }
    }
}