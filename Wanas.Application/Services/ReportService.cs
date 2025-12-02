using AutoMapper;
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
        private readonly IFileService _photoService;
        public ReportService(IMapper mapper ,IUnitOfWork unitOfWork, IAuditLogService auditLogService , IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _photoService = fileService;
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
            if (reportDto.Photos != null)
            {
                foreach (var photo in reportDto.Photos)
                {
                    var url = await _photoService.SaveFileAsync(photo);

                    var entity = new ReportPhoto
                    {
                        ReportId = report.ReportId,
                        URL = url
                    };

                    await _unitOfWork.ReportPhotos.AddAsync(entity);
                }

                await _auditLogService.LogAsync("ReportSubmitted", reporterId, reporterId, $"ReportId={report.ReportId}; TargetId={report.TargetId}; TargetType={report.TargetType}");

            }
                var finalReport = await _unitOfWork.Reports.GetByIdAsync(report.ReportId);
                var response = _mapper.Map<ReportResponseDto>(finalReport);
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
