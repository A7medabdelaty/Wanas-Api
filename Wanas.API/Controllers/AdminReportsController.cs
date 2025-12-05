using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wanas.Application.DTOs.Reports;
using Wanas.Application.Interfaces;
using Wanas.Domain.Enums;

namespace Wanas.API.Controllers
{
    [ApiController]
    [Route("api/admin/reports")]
    [Authorize(Roles = "Admin")]
    public class AdminReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        public AdminReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }
        [HttpGet]
        public async Task<IActionResult> GetReports([FromQuery] ReportStatus? status, [FromQuery] bool? escalated, [FromQuery] ReportSeverity? severity)
        {
            var reports = await _reportService.GetReportsAsync(status, escalated, severity);
            return Ok(new { totalCount = reports.Count(), reports });
        }

        // Dashboard counts endpoint
        [HttpGet("counts")]
        public async Task<IActionResult> GetCounts()
        {
            var all = await _reportService.GetReportsAsync();
            var total = all.Count();
            var pending = all.Count(r => r.Status == ReportStatus.Pending);
            var reviewed = all.Count(r => r.Status == ReportStatus.Reviewed);
            var resolved = all.Count(r => r.Status == ReportStatus.Resolved);
            var rejected = all.Count(r => r.Status == ReportStatus.Rejected);
            var escalated = all.Count(r => r.IsEscalated);

            // Percent of reviewing: reviewed / total (avoid division by zero)
            var reviewingPercent = total ==0 ?0 : Math.Round((double)reviewed / total *100,2);

            return Ok(new
            {
                total,
                pendingCount = pending,
                reviewedCount = reviewed,
                resolvedCount = resolved,
                rejectedCount = rejected,
                escalatedCount = escalated,
                reviewingPercent
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReport(int id)
        {
            var report = await _reportService.GetReportByIdAsync(id);
            if (report == null) return NotFound(new { message = "Report not found." });
            return Ok(report);
        }
        [HttpPost("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReportStatusDto dto)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var ok = await _reportService.UpdateReportStatusAsync(id, dto.NewStatus, adminId, dto.AdminNote, dto.Severity);
            if (!ok) return NotFound(new { message = "Report not found." });
            return Ok(new { message = "Status updated.", reportId = id });
        }
        [HttpPost("{id}/escalate")]
        public async Task<IActionResult> Escalate(int id, [FromBody] EscalateReportDto dto)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var ok = await _reportService.EscalateReportAsync(id, adminId, dto.Reason, dto.CancelEscalation);
            if (!ok) return NotFound(new { message = "Report not found." });
            return Ok(new { message = dto.CancelEscalation ? "Escalation removed." : "Report escalated.", reportId = id });
        }
    }
}