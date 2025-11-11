

using AutoMapper;
using Wanas.Application.DTOs.Reports;
using Wanas.Domain.Entities;

namespace Wanas.Application.Mappings
{
    public class ReportProfile:Profile
    {
        public ReportProfile()
        {
            // Map DTO to Entity for submission
            CreateMap<CreateReportDto, Report>()
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // Status is set in the service or by default value (Pending)
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // CreatedAt is set in the service or by the database
                .ForMember(dest => dest.ReporterId, opt => opt.Ignore()) // ReporterId comes from the service method parameter
                .ForMember(dest => dest.ReportPhotos, opt => opt.Ignore()); // Photos are mapped in the service logic

            // Map Entity back to Response DTO
            CreateMap<Report, ReportResponseDto>()
                // Manually map the collection of ReportPhotos to a list of URLs
                .ForMember(dest => dest.PhotoUrls, opt => opt.MapFrom(src => src.ReportPhotos.Select(p => p.URL)));

            //  Photo URL from the DTO to Entity
            CreateMap<string, ReportPhoto>()
                .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ReportId, opt => opt.Ignore())
                .ForMember(dest => dest.Report, opt => opt.Ignore());
        }
    }
}
