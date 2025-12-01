using AutoMapper;
using Wanas.Application.DTOs.Listing;
using Wanas.Domain.Entities;

namespace Wanas.Application.Mappings
{
    public class HostDetailsProfile : Profile
    {
        public HostDetailsProfile() 
        {
            CreateMap<ApplicationUser, HostDetailsDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.Photo))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Bio));
        }
    }
}
