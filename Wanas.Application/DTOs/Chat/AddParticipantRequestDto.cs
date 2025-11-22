
namespace Wanas.Application.DTOs.Chat
{
    public class AddParticipantRequestDto
    {
        public int ChatId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}
