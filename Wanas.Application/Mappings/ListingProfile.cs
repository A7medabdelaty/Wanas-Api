using AutoMapper;
using Wanas.Application.DTOs.Listing;
using Wanas.Domain.Entities;

namespace Wanas.Application.Mappings
{
    public class ListingProfile : Profile
    {
        public ListingProfile()
        {
            // Listing
            CreateMap<Listing, ListingDetailsDto>()
                .ForMember(dest => dest.ListingPhotos, opt => opt.MapFrom(src => src.ListingPhotos))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
                .ForMember(dest => dest.TotalRooms, opt => opt.MapFrom(src => src.ApartmentListing.TotalRooms))
                .ForMember(dest => dest.AvailableRooms, opt => opt.MapFrom(src => src.ApartmentListing.AvailableRooms))
                .ForMember(dest => dest.TotalBeds, opt => opt.MapFrom(src => src.ApartmentListing.TotalBeds))
                .ForMember(dest => dest.AvailableBeds, opt => opt.MapFrom(src => src.ApartmentListing.AvailableBeds));

            CreateMap<CreateListingDto, Listing>()
                .ForMember(dest => dest.ApartmentListing, opt => opt.MapFrom(src => src));

            CreateMap<UpdateListingDto, Listing>()
                .ForMember(dest => dest.ApartmentListing, opt => opt.MapFrom(src => src));

            // ApartmentListing -> handled via Listing mapping
            CreateMap<CreateListingDto, ApartmentListing>()
                .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.Rooms));

            CreateMap<UpdateListingDto, ApartmentListing>()
                .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.Rooms));

            // ListingPhoto
            CreateMap<ListingPhoto, ListingPhotoDto>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.URL));

            CreateMap<ListingPhotoDto, ListingPhoto>()
                .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.Url));

            // Room
            CreateMap<Room, CreateRoomDto>()
                .ForMember(dest => dest.Beds, opt => opt.MapFrom(src => src.Beds));

            CreateMap<CreateRoomDto, Room>()
                .ForMember(dest => dest.Beds, opt => opt.MapFrom(src => src.Beds));

            CreateMap<UpdateRoomDto, Room>();

            // Bed
            CreateMap<Bed, BedDto>();
            CreateMap<BedDto, Bed>();
        }
    }
}
