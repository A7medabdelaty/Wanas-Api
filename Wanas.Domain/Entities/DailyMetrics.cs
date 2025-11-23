namespace Wanas.Domain.Entities
{
    public class DailyMetrics
    {
        public int Id { get; set; } 
        public DateOnly Date { get; set; }
        public int TotalListings { get; set; }
        public int PendingListings { get; set; }
        public int ApprovedListings { get; set; }
        public int RejectedListings { get; set; }
        public int FlaggedListings { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; } // distinct traffic users
        public int Requests { get; set; }
        public DateTime LastCalculatedAt { get; set; } = DateTime.UtcNow;
    }
}