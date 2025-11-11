using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public MessageType Type { get; set; }
        public string? TextContent { get; set; }
        public string? MediaUrl { get; set; }
        public string? MediaMimeType { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public string SenderId { get; set; }
        public virtual ApplicationUser Sender { get; set; }

        public int ChatId { get; set; }
        public virtual Chat Chat { get; set; }

        public virtual ICollection<MessageReadReceipt> ReadReceipts { get; set; } = new List<MessageReadReceipt>();
    }
}
