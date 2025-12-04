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
                .ForMember(d => d.BedIds,
                    opt => opt.MapFrom(src => src.Beds.Select(b => b.BedId).ToList()));
        }
    }
}
