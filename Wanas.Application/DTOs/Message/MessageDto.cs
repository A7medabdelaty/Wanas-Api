namespace Wanas.Application.DTOs.Message
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
