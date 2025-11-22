namespace Wanas.Application.DTOs.Chat
{
    public class CreateChatRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string? ChatName { get; set; }
        public bool IsGroup { get; set; }
    }
}
