using AutoMapper;
using Wanas.Application.DTOs.Listing;
using Wanas.Application.DTOs.Search;
using Wanas.Domain.Entities;

namespace Wanas.Application.Mappings
{
    public class ListingProfile : Profile
    {
        public ListingProfile()
        {
            CreateMap<Listing, ListingCardDto>()
                .ForMember(dest => dest.MainPhotoUrl,
                    opt => opt.MapFrom(src =>
                        src.ListingPhotos != null
                            ? src.ListingPhotos.OrderBy(p => p.Id).Select(p => p.URL).FirstOrDefault()
                            : null))
                .ForMember(dest => dest.AvailableRooms,
                    opt => opt.MapFrom(src => src.ApartmentListing.AvailableRooms))
                .ForMember(dest => dest.AvailableBeds,
                    opt => opt.MapFrom(src => src.ApartmentListing.AvailableBeds))
                .ForMember(dest => dest.HasInternet,
                    opt => opt.MapFrom(src => src.ApartmentListing.HasInternet));
        }
    }
}
