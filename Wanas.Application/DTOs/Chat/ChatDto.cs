namespace Wanas.Application.DTOs.Chat
{
    public class ChatDto
    {
        public int Id { get; set; }
        public bool IsGroup { get; set; }
        public string ChatName { get; set; }
        public int? ListingId { get; set; }
        public ICollection<ChatParticipantDto> Participants { get; set; }
    }
}
