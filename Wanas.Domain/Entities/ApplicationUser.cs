using Microsoft.AspNetCore.Identity;
using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        //Mandatory Fields
        public string FullName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public ProfileType ProfileType { get; set; } = ProfileType.Renter;

        //Optional Fields
        public string? Bio { get; set; } 
        public int? Age { get; set; }
        public string? Photo { get; set; }

        // System Fields
        public new string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Admin Management Properties
        public bool IsDeleted { get; set; } = false;
        public bool IsSuspended { get; set; }
        public DateTime? SuspendedUntil { get; set; }
        public bool IsBanned { get; set; }
        public bool IsVerified { get; set; }

        // Completion Flags
        public bool IsProfileCompleted { get; set; } = false;
        public bool IsPreferenceCompleted { get; set; } = false;
        public bool IsFirstLogin { get; set; } = true;

        // Navigation Properties
        public virtual UserPreference? UserPreference { get; set; }
        public HashSet<Bed>? Beds { get; set; } = new();
        public ICollection<Listing> Listings { get; set; } = new List<Listing>();
        public ICollection<Match> Matches { get; set; } = new List<Match>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();
        public ICollection<MessageReadReceipt> MessageReadReceipts { get; set; } = new List<MessageReadReceipt>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();

        // Appeal relationships
        public ICollection<Appeal> Appeals { get; set; } = new List<Appeal>();
        public ICollection<Appeal> ReviewedAppeals { get; set; } = new List<Appeal>();
        public List<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
