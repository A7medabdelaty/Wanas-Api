
using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    public class Report
    {
        public int ReportId { get; set; }
        public ReportTarget TargetType { get; set; }
        public string TargetId { get; set; }
        public string Reason { get; set; }
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
        public DateTime CreatedAt { get; set; }
        public string ReporterId { get; set; }
        public ApplicationUser Reporter { get; set; }
    }

}
