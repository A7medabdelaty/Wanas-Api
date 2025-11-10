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
        public virtual ApplicationUser Reporter { get; set; } = null!;
        public virtual ICollection<ReportPhoto> ReportPhotos { get; set; } = new HashSet<ReportPhoto>();
    }
}
