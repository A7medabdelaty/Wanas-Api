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
            // Chat → ChatDto
            CreateMap<Chat, ChatDto>()
                .ForMember(dest => dest.ChatName,
                    opt => opt.MapFrom(src =>
                        !string.IsNullOrWhiteSpace(src.Name)
                            ? src.Name
                            : $"Chat #{src.Id}"))
                .ForMember(dest => dest.Participants,
                    opt => opt.MapFrom(src => src.ChatParticipants));

            // ChatParticipant → ChatParticipantDto
            CreateMap<ChatParticipant, ChatParticipantDto>()
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.User.Photo));

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
