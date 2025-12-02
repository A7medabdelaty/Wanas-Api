using AutoMapper;
using Wanas.Application.DTOs.Listing;
using Wanas.Domain.Entities;

namespace Wanas.Application.Mappings
{
    public class ListingProfile : Profile
    {
        public ListingProfile()
        {
            CreateMap<Listing, ListingDetailsDto>()
                .ForMember(dest=>dest.OwnerId, opt=>opt.MapFrom(src=>src.UserId))
                .ForMember(dest=>dest.GroupChatId, opt=>opt.MapFrom(src=>src.GroupChatId))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.ApartmentListing.Address))
                .ForMember(dest => dest.MonthlyPrice, opt => opt.MapFrom(src => src.ApartmentListing.MonthlyPrice))
                .ForMember(dest => dest.HasElevator, opt => opt.MapFrom(src => src.ApartmentListing.HasElevator))
                .ForMember(dest => dest.Floor, opt => opt.MapFrom(src => src.ApartmentListing.Floor))
                .ForMember(dest => dest.AreaInSqMeters, opt => opt.MapFrom(src => src.ApartmentListing.AreaInSqMeters))
                .ForMember(dest => dest.TotalBathrooms, opt => opt.MapFrom(src => src.ApartmentListing.TotalBathrooms))
                .ForMember(dest => dest.HasKitchen, opt => opt.MapFrom(src => src.ApartmentListing.HasKitchen))
                .ForMember(dest => dest.HasInternet, opt => opt.MapFrom(src => src.ApartmentListing.HasInternet))
                .ForMember(dest => dest.HasAirConditioner, opt => opt.MapFrom(src => src.ApartmentListing.HasAirConditioner))
                .ForMember(dest => dest.HasFans, opt => opt.MapFrom(src => src.ApartmentListing.HasFans))
                .ForMember(dest => dest.IsPetFriendly, opt => opt.MapFrom(src => src.ApartmentListing.IsPetFriendly))
                .ForMember(dest => dest.IsSmokingAllowed, opt => opt.MapFrom(src => src.ApartmentListing.IsSmokingAllowed))

    // ---------- COMPUTED FIELDS ----------
        .ForMember(dest => dest.TotalRooms, opt =>
            opt.MapFrom(src => src.ApartmentListing.Rooms.Count))

        .ForMember(dest => dest.AvailableRooms, opt =>
            opt.MapFrom(src =>
                src.ApartmentListing.Rooms.Count(r =>
                    r.Beds.Any(b => b.IsAvailable)
                )
            )
        )

        .ForMember(dest => dest.TotalBeds, opt =>
            opt.MapFrom(src =>
                src.ApartmentListing.Rooms.Sum(r => r.Beds.Count)
            )
        )

        .ForMember(dest => dest.AvailableBeds, opt =>
            opt.MapFrom(src =>
                src.ApartmentListing.Rooms.Sum(r =>
                    r.Beds.Count(b => b.IsAvailable)
                )
            )
        )
        .ForMember(dest => dest.ListingPhotos, opt => opt.MapFrom(src => src.ListingPhotos))
        .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments));


            //   LISTING PHOTO → DTO
            CreateMap<ListingPhoto, ListingPhotoDto>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.URL));


            //   CREATE LISTING DTO → ENTITY (with nested objects)
            CreateMap<CreateListingDto, Listing>()
                .ForMember(dest => dest.ApartmentListing, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.ListingPhotos, opt => opt.Ignore()) // handled manually
                .ForMember(dest => dest.Comments, opt => opt.Ignore())     // ignore comments
                .ForMember(dest => dest.Payments, opt => opt.Ignore());

            CreateMap<CreateListingDto, ApartmentListing>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.MonthlyPrice, opt => opt.MapFrom(src => src.MonthlyPrice))
                .ForMember(dest => dest.HasElevator, opt => opt.MapFrom(src => src.HasElevator))
                .ForMember(dest => dest.Floor, opt => opt.MapFrom(src => src.Floor))
                .ForMember(dest => dest.AreaInSqMeters, opt => opt.MapFrom(src => src.AreaInSqMeters))
                .ForMember(dest => dest.TotalBathrooms, opt => opt.MapFrom(src => src.TotalBathrooms))
                .ForMember(dest => dest.HasKitchen, opt => opt.MapFrom(src => src.HasKitchen))
                .ForMember(dest => dest.HasInternet, opt => opt.MapFrom(src => src.HasInternet))
                .ForMember(dest => dest.HasAirConditioner, opt => opt.MapFrom(src => src.HasAirConditioner))
                .ForMember(dest => dest.HasFans, opt => opt.MapFrom(src => src.HasFans))
                .ForMember(dest => dest.IsPetFriendly, opt => opt.MapFrom(src => src.IsPetFriendly))
                .ForMember(dest => dest.IsSmokingAllowed, opt => opt.MapFrom(src => src.IsSmokingAllowed))
                .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.Rooms))
                .ForMember(dest => dest.Beds, opt => opt.Ignore()); // Beds created from Rooms

            CreateMap<CreateRoomDto, Room>()
                .ForMember(dest => dest.Beds, opt => opt.MapFrom(src => src.Beds));

            CreateMap<BedDto, Bed>();



            //   UPDATE LISTING DTO → ENTITY
            CreateMap<UpdateListingDto, Listing>()
                .ForMember(dest => dest.ApartmentListing, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.ListingPhotos, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore());

            CreateMap<UpdateListingDto, ApartmentListing>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.MonthlyPrice, opt => opt.MapFrom(src => src.MonthlyPrice))
                .ForMember(dest => dest.HasElevator, opt => opt.MapFrom(src => src.HasElevator))
                .ForMember(dest => dest.Floor, opt => opt.MapFrom(src => src.Floor))
                .ForMember(dest => dest.AreaInSqMeters, opt => opt.MapFrom(src => src.AreaInSqMeters))
                .ForMember(dest => dest.TotalBathrooms, opt => opt.MapFrom(src => src.TotalBathrooms))
                .ForMember(dest => dest.HasKitchen, opt => opt.MapFrom(src => src.HasKitchen))
                .ForMember(dest => dest.HasInternet, opt => opt.MapFrom(src => src.HasInternet))
                .ForMember(dest => dest.HasAirConditioner, opt => opt.MapFrom(src => src.HasAirConditioner))
                .ForMember(dest => dest.HasFans, opt => opt.MapFrom(src => src.HasFans))
                .ForMember(dest => dest.IsPetFriendly, opt => opt.MapFrom(src => src.IsPetFriendly))
                .ForMember(dest => dest.IsSmokingAllowed, opt => opt.MapFrom(src => src.IsSmokingAllowed))
                .ForMember(dest => dest.Rooms, opt => opt.Ignore())  // manually updated
                .ForMember(dest => dest.Beds, opt => opt.Ignore());


            // Update room (handled manually in service later)
            CreateMap<UpdateRoomDto, Room>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcVal) => srcVal != null));

            // LISTING → LISTING CARD DTO
            CreateMap<Listing, ListingCardDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.MonthlyPrice, opt => opt.MapFrom(src => src.ApartmentListing.MonthlyPrice))
                .ForMember(dest => dest.MainPhotoUrl, opt =>
                    opt.MapFrom(src => src.ListingPhotos.FirstOrDefault() != null
                        ? src.ListingPhotos.First().URL
                        : null
                    )
                )
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))

                .ForMember(dest => dest.AvailableRooms, opt =>
                    opt.MapFrom(src =>
                        src.ApartmentListing.Rooms.Count(r =>
                            r.Beds.Any(b => b.IsAvailable)
                        )
                    )
                )
                .ForMember(dest => dest.AvailableBeds, opt =>
                    opt.MapFrom(src =>
                        src.ApartmentListing.Rooms.Sum(r =>
                            r.Beds.Count(b => b.IsAvailable)
                        )
                    )
                )
                .ForMember(dest => dest.HasInternet, opt => opt.MapFrom(src => src.ApartmentListing.HasInternet));


            // REVERSE MAPS
            CreateMap<ApartmentListing, CreateListingDto>().ReverseMap();
            CreateMap<Room, CreateRoomDto>().ReverseMap();
            CreateMap<Bed, BedDto>().ReverseMap();

            CreateMap<ApartmentListing, UpdateListingDto>().ReverseMap();
            CreateMap<Room, UpdateRoomDto>().ReverseMap();
        }
    }

}
