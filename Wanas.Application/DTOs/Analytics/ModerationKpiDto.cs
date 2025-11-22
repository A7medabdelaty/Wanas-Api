namespace Wanas.Application.DTOs.Analytics
{
    public class ModerationKpiDto
    {
        public DateOnly Date { get; set; }
        public double AvgApprovalTimeHours { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int PendingCount { get; set; }
        public int FlaggedCount { get; set; }
    }
}