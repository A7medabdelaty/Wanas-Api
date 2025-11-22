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