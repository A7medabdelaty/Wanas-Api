using AutoMapper;
using Wanas.Application.DTOs.Listing;
using Wanas.Domain.Entities;

namespace Wanas.Application.Mappings
{
    public class ListingProfile : Profile
    {
        public ListingProfile()
        {
            // Listing → ListingDetailsDto (flatten ApartmentListing)
            CreateMap<Listing, ListingDetailsDto>()
                .ForMember(dest => dest.TotalRooms, opt => opt.MapFrom(src => src.ApartmentListing.TotalRooms))
                .ForMember(dest => dest.AvailableRooms, opt => opt.MapFrom(src => src.ApartmentListing.AvailableRooms))
                .ForMember(dest => dest.TotalBeds, opt => opt.MapFrom(src => src.ApartmentListing.TotalBeds))
                .ForMember(dest => dest.AvailableBeds, opt => opt.MapFrom(src => src.ApartmentListing.AvailableBeds))
                .ForMember(dest => dest.TotalBathrooms, opt => opt.MapFrom(src => src.ApartmentListing.TotalBathrooms))
                .ForMember(dest => dest.AreaInSqMeters, opt => opt.MapFrom(src => src.ApartmentListing.AreaInSqMeters))
                .ForMember(dest => dest.Floor, opt => opt.MapFrom(src => src.ApartmentListing.Floor))
                .ForMember(dest => dest.HasElevator, opt => opt.MapFrom(src => src.ApartmentListing.HasElevator))
                .ForMember(dest => dest.HasKitchen, opt => opt.MapFrom(src => src.ApartmentListing.HasKitchen))
                .ForMember(dest => dest.HasInternet, opt => opt.MapFrom(src => src.ApartmentListing.HasInternet))
                .ForMember(dest => dest.ListingPhotos, opt => opt.MapFrom(src => src.ListingPhotos))
                .ForMember(dest => dest.Comments, opt => opt.Ignore()); // Excluded


            // CreateListingDto → Listing + ApartmentListing
            CreateMap<CreateListingDto, Listing>()
                .ForMember(dest => dest.ApartmentListing, opt => opt.MapFrom(src => src));

            CreateMap<CreateListingDto, ApartmentListing>()
                .ForMember(dest => dest.Rooms, opt => opt.Ignore())
                .ForMember(dest => dest.Beds, opt => opt.Ignore());


            // UpdateListingDto → Listing + ApartmentListing
            CreateMap<UpdateListingDto, Listing>()
                .ForMember(dest => dest.ApartmentListing, opt => opt.MapFrom(src => src));

            CreateMap<UpdateListingDto, ApartmentListing>();


            // ListingPhoto ↔ ListingPhotoDto
            CreateMap<ListingPhoto, ListingPhotoDto>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.URL));

            CreateMap<ListingPhotoDto, ListingPhoto>()
                .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.Url));
        }
    }
}
