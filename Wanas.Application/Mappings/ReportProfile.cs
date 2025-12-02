using AutoMapper;
using Wanas.Application.DTOs.Reports;
using Wanas.Domain.Entities;

namespace Wanas.Application.Mappings
{
    public class ReportProfile : Profile
    {
        public ReportProfile()
        {
            // Map DTO to Entity for submission
            CreateMap<CreateReportDto, Report>()
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // Status is set in the service or by default value (Pending)
                    .ForMember(dest => dest.ReportPhotos, opt => opt.Ignore())
                //.ForMember(dest => dest.ReportPhotos,
                //           opt => opt.MapFrom(src =>
                //                (src.PhotoUrls ?? new List<string>())
                //                    .Select(url => new ReportPhoto { URL = url }).ToList()))

                .ForMember(dest => dest.CreatedAt,
                           opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Reporter, opt => opt.Ignore()) // set in service (attach user)
                .ForMember(dest => dest.ReporterId, opt => opt.Ignore()) // set in service from current user
                .ForMember(dest => dest.ReportId, opt => opt.Ignore()); // DB generates id


            // Report -> ReportResponseDto (for sending to clients)
            CreateMap<Report, ReportResponseDto>()
                .ForMember(dest => dest.PhotoUrls,
                           opt => opt.MapFrom(src => src.ReportPhotos.Select(p => p.URL).ToList()))
                .ForMember(dest => dest.ReorterId,
                           opt => opt.MapFrom(src => src.ReporterId));
            //  Photo URL from the DTO to Entity
            CreateMap<string, ReportPhoto>()
                .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ReportId, opt => opt.Ignore())
                .ForMember(dest => dest.Report, opt => opt.Ignore());
        }
    }
}
