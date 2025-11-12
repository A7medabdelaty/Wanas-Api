using AutoMapper;
using Wanas.Application.DTOs.Chat;
using Wanas.Application.DTOs.Message;
using Wanas.Domain.Entities;

namespace Wanas.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Chat, ChatDto>()
                .ForMember(dest => dest.ParticipantIds,
                    opt => opt.MapFrom(src => src.ChatParticipants.Select(p => p.UserId)))
                .ForMember(dest => dest.Messages,
                    opt => opt.MapFrom(src => src.Messages));

            CreateMap<CreateChatRequestDto, Chat>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.SenderId,
                    opt => opt.MapFrom(src => src.SenderId))
                .ForMember(dest => dest.Content,
                    opt => opt.MapFrom(src => src.TextContent))
                .ForMember(dest => dest.SentAt,
                    opt => opt.MapFrom(src => src.SentAt));

            CreateMap<CreateMessageRequestDto, Message>();
        }
    }
}
