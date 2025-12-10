using AutoMapper;
using Wanas.Application.DTOs.Review;
using Wanas.Domain.Entities;

namespace Wanas.Application.Mappings
{
    public class ReviewProfile : Profile
    {
        public ReviewProfile()
        {
            CreateMap<CreateReviewDto, Review>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UpdateReviewDto, Review>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.ReviewerName,
                    opt => opt.MapFrom(src => src.Reviewer.FullName))
                .ForMember(dest => dest.ReviewerProfilePhotoUrl,
                    opt => opt.MapFrom(src => src.Reviewer.Photo))
                .ForMember(dest => dest.ReviewerId,
                    opt => opt.MapFrom(src => src.Reviewer.Id));

        }
    }
}
