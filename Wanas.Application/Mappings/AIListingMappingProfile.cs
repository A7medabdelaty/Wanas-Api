using AutoMapper;
using Wanas.Application.DTOs.AI;
using Wanas.Domain.Entities;

namespace Wanas.Application.Mappings
{
    public class AIListingMappingProfile : Profile
    {
        public AIListingMappingProfile()
        {
            CreateMap<GeneratedListingResponseDto, Listing>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.MonthlyPrice, opt => opt.MapFrom(src => src.SuggestionMonthlyPrice))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                   .ForMember(dest => dest.ListingPhotos,
                       opt => opt.MapFrom(src =>
                           (src.SuggestedPhotoUrls ?? new List<string>())
                             .Select(u => new ListingPhoto { URL = u }).ToHashSet()));
                //.ForAllOtherMembers(opt => opt.Ignore()); for tags and amenities which are not mapped currently


         }
    }
    
}
