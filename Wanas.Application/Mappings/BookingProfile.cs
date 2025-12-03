using AutoMapper;
using Wanas.Application.DTOs.Booking;
using Wanas.Application.DTOs.Listing;
using Wanas.Application.DTOs.Payment;
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

            CreateMap<ReserveBedsRequestDto, Reservation>()
                        .ForMember(dest => dest.Id, opt => opt.Ignore())
                        .ForMember(dest => dest.ReservedBeds, opt => opt.Ignore())
                        .ForMember(dest => dest.DepositAmount, opt => opt.Ignore())
                        .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
                        .ForMember(dest => dest.Status, opt => opt.Ignore());
        }
    }
}
