namespace Wanas.Application.DTOs.Chat
{
    public class ChatSummaryDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsGroup { get; set; }
        public string? LastMessageContent { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }
}
