namespace Wanas.Application.DTOs.Message
{
    public class CreateMessageRequestDto
    {
        public int ChatId { get; set; }
        public string? SenderId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
