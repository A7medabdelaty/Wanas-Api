using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.Reports
{
    public class UpdateReportStatusDto
    {
        public ReportStatus NewStatus { get; set; }
        public string? AdminNote { get; set; }
        public ReportSeverity? Severity { get; set; }
    }
}