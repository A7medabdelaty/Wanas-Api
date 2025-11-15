using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanas.Application.DTOs.Reports;

namespace Wanas.Application.Interfaces
{
    public interface IReportService
    {
        Task<ReportResponseDto> SubmitReportAsync(CreateReportDto reportDto, string reporterId);

        //Maybe use them later if ...if we treat with admin 
        // Task<IEnumerable<ReportResponseDto>> GetAllReportsAsync(); 
        // Task<ReportResponseDto?> GetReportByIdAsync(int reportId);
        // Task UpdateReportStatusAsync(int reportId, ReportStatus newStatus);
    }
}
