using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanas.Application.DTOs.Reports;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        public ReportService(IMapper mapper ,IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _auditLogService = auditLogService;
        }

        //public async Task<ReportResponseDto> CreateREportAsync(CreateReportDto dto, string reporterId)
        //{
        //    var report = _mapper.Map<Report>(dto);
        //    report.ReporterId = reporterId;
        //    // save using unit of work / repository
        //    // await _uow.Reports.AddAsync(report);
        //    // await _uow.SaveAsync();
        //    await _unitOfWork.Reports.AddAsync(report);
        //    return null;

           
        //}
        public async Task<ReportResponseDto> SubmitReportAsync(CreateReportDto reportDto, string reporterId)
        {
            var report = _mapper.Map<Report>(reportDto);
            report.ReporterId = reporterId;
            report.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Reports.AddAsync(report);
            await _unitOfWork.CommitAsync();
            if (report.ReportPhotos != null && reportDto.PhotoUrls != null && reportDto.PhotoUrls.Any())
            {
                foreach (var url in reportDto.PhotoUrls)
                {
                    var photo = new ReportPhoto
                    {
                        URL = url,
                        ReportId = report.ReportId //linking photo to report by reporterId

                    };

                    await _unitOfWork.ReportPhotos.AddAsync(photo);
                }

                await _unitOfWork.CommitAsync();
            }
            var finalReport = await _unitOfWork.Reports.GetByIdAsync(report.ReportId);
            var response = _mapper.Map<ReportResponseDto>(finalReport);

            await _auditLogService.LogAsync("ReportSubmitted", reporterId, reporterId, $"ReportId={report.ReportId}; TargetId={report.TargetId}; TargetType={report.TargetType}");
            return response;



        }

        // New methods for admin visibility
        public async Task<IEnumerable<ReportResponseDto>> GetReportsAsync(ReportStatus? status = null, bool? escalated = null, ReportSeverity? severity = null)
        {
            var reports = await _unitOfWork.Reports.FindAsync(r =>
                (status == null || r.Status == status) &&
                (escalated == null || r.IsEscalated == escalated) &&
                (severity == null || r.Severity == severity));
            // Include photos manually if needed (Generic repo returns no includes) -> fallback: mapping handles collection if loaded.
            return reports.Select(r => _mapper.Map<ReportResponseDto>(r));
        }
        public async Task<ReportResponseDto?> GetReportByIdAsync(int reportId)
        {
            var report = await _unitOfWork.Reports.GetByIdAsync(reportId);
            return report == null ? null : _mapper.Map<ReportResponseDto>(report);
        }
        public async Task<bool> UpdateReportStatusAsync(int reportId, ReportStatus newStatus, string? adminId, string? adminNote, ReportSeverity? severity)
        {
            var report = await _unitOfWork.Reports.GetByIdAsync(reportId);
            if (report == null) return false;
            report.Status = newStatus;
            report.ReviewedByAdminId = adminId;
            report.ReviewedAt = DateTime.UtcNow;
            if (adminNote != null) report.AdminNote = adminNote;
            if (severity.HasValue) report.Severity = severity.Value;
            _unitOfWork.Reports.Update(report);
            await _unitOfWork.CommitAsync();
            if (!string.IsNullOrEmpty(adminId))
            {
                await _auditLogService.LogAsync("ReportStatusUpdated", adminId, report.ReporterId, $"ReportId={reportId}; NewStatus={newStatus}; Severity={report.Severity}; Note={adminNote}");
            }
            return true;
        }
        public async Task<bool> EscalateReportAsync(int reportId, string adminId, string? reason, bool cancel = false)
        {
            var report = await _unitOfWork.Reports.GetByIdAsync(reportId);
            if (report == null) return false;
            if (cancel)
            {
                report.IsEscalated = false;
                report.EscalatedAt = null;
                report.EscalationReason = null;
            }
            else
            {
                report.IsEscalated = true;
                report.EscalatedAt = DateTime.UtcNow;
                report.EscalationReason = reason;
            }
            report.ReviewedByAdminId = adminId; // track who escalated
            _unitOfWork.Reports.Update(report);
            await _unitOfWork.CommitAsync();
            var action = cancel ? "ReportDeEscalated" : "ReportEscalated";
            await _auditLogService.LogAsync(action, adminId, report.ReporterId, $"ReportId={reportId}; Reason={reason}");
            return true;
        }
    }
}
