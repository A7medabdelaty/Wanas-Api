
namespace Wanas.Domain.Entities
{
    public class ChatParticipant
    {
        public int Id { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; } 
        public bool IsAdmin { get; set; }

        public int ChatId { get; set; }
        public virtual Chat Chat { get; set; }

        public int UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
