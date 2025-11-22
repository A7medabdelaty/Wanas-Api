namespace Wanas.Application.DTOs.Analytics
{
    public class AnalyticsSummaryDto
    {
        public DateOnly Date { get; set; }
        public int TotalListings { get; set; }
        public int PendingListings { get; set; }
        public int ApprovedListings { get; set; }
        public int RejectedListings { get; set; }
        public int FlaggedListings { get; set; }
        public int ActiveUsers { get; set; }
        public int Requests { get; set; }
    }
}