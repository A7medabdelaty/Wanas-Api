using Microsoft.AspNetCore.Identity;
using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    // needs configuration
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public ProfileType ProfileType { get; set; }
        public int Age { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Photo { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
        public virtual UserPreference UserPreference { get; set; } = null!;
        public HashSet<Bed>? Beds { get; set; } = new();
        public ICollection<Listing> Listings { get; set; } = new List<Listing>();
        public ICollection<Match> Matches { get; set; } = new List<Match>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();
        public ICollection<MessageReadReceipt> MessageReadReceipts { get; set; } = new List<MessageReadReceipt>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();

        public List<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
