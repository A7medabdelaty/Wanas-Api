
namespace Wanas.Domain.Entities
{
    public class MessageReadReceipt
    {
        public int Id { get; set; }
        public DateTime? ReadAt { get; set; } 
        public int MessageId { get; set; }
        public Message Message { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
