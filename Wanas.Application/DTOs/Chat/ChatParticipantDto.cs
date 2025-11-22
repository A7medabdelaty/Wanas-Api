namespace Wanas.Application.DTOs.Chat
{
    public class ChatParticipantDto
    {
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
