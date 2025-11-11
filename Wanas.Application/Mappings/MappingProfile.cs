using Wanas.Application.DTOs.Message;
using Wanas.Domain.Entities;
using AutoMapper;

namespace Wanas.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map domain Message -> MessageDto
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.TextContent));

            CreateMap<MessageDto, Message>()
                .ForMember(dest => dest.TextContent, opt => opt.MapFrom(src => src.Content));
        }
    }
}
