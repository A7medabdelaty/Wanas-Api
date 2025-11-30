namespace Wanas.Application.DTOs.Chat
{
    public class ChatParticipantDto
    {
        public string UserId { get; set; }
        public string? UserName { get; set; }
        public string? DisplayName { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
