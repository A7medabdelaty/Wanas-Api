using AutoMapper;
using Wanas.Application.DTOs.Booking;
using Wanas.Application.DTOs.Listing;
using Wanas.Application.DTOs.Payment;
using Wanas.Application.DTOs.Reservation;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;

namespace Wanas.Application.Mappings
{
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<int, MockPaymentResultDto>()
            .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.Success, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.PaidAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<Reservation, ReservationDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ListingId, opt => opt.MapFrom(src => src.ListingId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.DepositAmount, opt => opt.MapFrom(src => src.DepositAmount))
                .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.RemainingAmount))
                .ForMember(dest => dest.PaidAt, opt => opt.MapFrom(src => src.PaidAt))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.FromDate, opt => opt.MapFrom(src => src.FromDate))
                .ForMember(dest => dest.ToDate, opt => opt.MapFrom(src => src.ToDate))

                .ForMember(dest => dest.ListingTitle,
                    opt => opt.MapFrom(src => src.Listing != null ? src.Listing.Title : null))
                .ForMember(dest => dest.OwnerId,
                    opt => opt.MapFrom(src => src.Listing != null ? src.Listing.UserId : null))

                // Beds
                .ForMember(dest => dest.Beds, opt => opt.MapFrom(src => src.Beds));

            CreateMap<Reservation, ReservationListItemDto>()
                .ForMember(dest => dest.ListingTitle, opt => opt.MapFrom(src => src.Listing.Title))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Listing.City))
                .ForMember(dest => dest.CoverPhotoUrl,
                           opt => opt.MapFrom(src => src.Listing.ListingPhotos))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.FromDate))
                .ForMember(dest => dest.DurationInDays, 
                           opt => opt.MapFrom(src => (src.ToDate - src.FromDate).Days))
                .ForMember(dest => dest.Beds, opt => opt.MapFrom(src => src.Beds));
        }
    }
}
