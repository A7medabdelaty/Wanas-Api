using Microsoft.AspNetCore.Identity;

namespace Wanas.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string City { get; set; }
        public string Bio { get; set; }
        public string ProfileType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Photo { get; set; }
        public bool IsDeleted { get; set; } = false; 
        public ICollection<Listing> Listings { get; set; }
        public ICollection<Match> Matches { get; set; } = new List<Match>();
        public HashSet<Bed>? Beds { get; set; } = new();
        public virtual Preference Preference { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();
        public ICollection<MessageReadReceipt> MessageReadReceipts { get; set; } = new List<MessageReadReceipt>();

        
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
