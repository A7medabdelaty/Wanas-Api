using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanas.Application.DTOs.Reports;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;

namespace Wanas.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IMapper _mapper;
        public ReportService(IMapper mapper /*, IUnitOfWork uow, IUserAccessor ... */)
        {
            _mapper = mapper;
        }

        public async Task<ReportResponseDto> CreateREportAsync(CreateReportDto dto, string reporterId)
        {
            var report = _mapper.Map<Report>(dto);
            report.ReporterId = reporterId;
            // save using unit of work / repository
            // await _uow.Reports.AddAsync(report);
            // await _uow.SaveAsync();
        }
        public Task<ReportResponseDto> SubmitReportAsync(CreateReportDto reportDto, string reporterId)
        {
            throw new NotImplementedException();
        }
    }
}
