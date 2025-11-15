using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanas.Application.DTOs.Reports;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ReportService(IMapper mapper ,IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ReportResponseDto> CreateREportAsync(CreateReportDto dto, string reporterId)
        {
            var report = _mapper.Map<Report>(dto);
            report.ReporterId = reporterId;
            // save using unit of work / repository
            // await _uow.Reports.AddAsync(report);
            // await _uow.SaveAsync();
            await _unitOfWork.Reports.AddAsync(report);
            return null;

           
        }
        public async Task<ReportResponseDto> SubmitReportAsync(CreateReportDto reportDto, string reporterId)
        {
            var report = _mapper.Map<Report>(reportDto);
            report.ReporterId = reporterId;
            report.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Reports.AddAsync(report);
            await _unitOfWork.CommitAsync();
            if(report.ReportPhotos != null && reportDto.PhotoUrls.Any())
            {
                foreach (var url in reportDto.PhotoUrls)
                {
                    var photo = new ReportPhoto
                    {
                        URL = url,
                        ReportId = report.ReportId
                    };

                    await _unitOfWork.ReportPhotos.AddAsync(photo);
                }

                await _unitOfWork.CommitAsync();
            }
            var finalReport = await _unitOfWork.Reports.GetByIdAsync(report.ReportId);
            var response = _mapper.Map<ReportResponseDto>(finalReport);
            return response;



        }
    }
}
