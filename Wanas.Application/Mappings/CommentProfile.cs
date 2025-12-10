using AutoMapper;
using Wanas.Application.DTOs.Listing;
using Wanas.Domain.Entities;

namespace Wanas.Application.Mappings
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.FullName))
                .ForMember(dest => dest.AuthorPhoto, opt => opt.MapFrom(src => src.Author.Photo))
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies))
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src=>src.Author.Id));

            CreateMap<CreateCommentDto, Comment>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UpdateCommentDto, Comment>();
        }
    }
}
