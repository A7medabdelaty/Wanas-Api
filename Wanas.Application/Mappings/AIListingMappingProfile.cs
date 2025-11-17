using AutoMapper;
using Wanas.Application.DTOs.AI;
using Wanas.Domain.Entities;

namespace Wanas.Application.Mappings
{
    public class AIListingMappingProfile:Profile
    {
        public AIListingMappingProfile()
        {
           CreateMap<GeneratedListingResponseDto,Listing>()
                .ForMember(dest=> dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest=> dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.PricePerMonth, opt => opt.MapFrom(src => src.))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.RoomType))
        }
    }
}
