namespace Wanas.Application.DTOs.Chat
{
    public class ChatDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsGroup { get; set; }
        public List<string> ParticipantIds { get; set; } = new();
    }
}
