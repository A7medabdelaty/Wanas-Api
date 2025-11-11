
namespace Wanas.Domain.Entities
{
    public class MessageReadReceipt
    {
        public int Id { get; set; }
        public DateTime? ReadAt { get; set; } 

        public int MessageId { get; set; }
        public virtual Message Message { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
