using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanas.Application.DTOs.Reports;
using Wanas.Domain.Enums;

namespace Wanas.Application.Interfaces
{
    public interface IReportService
    {
        Task<ReportResponseDto> SubmitReportAsync(CreateReportDto reportDto, string reporterId);

        // Admin operations
        Task<IEnumerable<ReportResponseDto>> GetReportsAsync(ReportStatus? status = null, bool? escalated = null, ReportSeverity? severity = null);
        Task<ReportResponseDto?> GetReportByIdAsync(int reportId);
        Task<bool> UpdateReportStatusAsync(int reportId, ReportStatus newStatus, string? adminId, string? adminNote, ReportSeverity? severity);
        Task<bool> EscalateReportAsync(int reportId, string adminId, string? reason, bool cancel = false);
    }
}
