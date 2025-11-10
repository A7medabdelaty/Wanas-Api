using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    public class Match
    {
        public int MatchId { get; set; }
        public double MatchScore { get; set; }
        public DateTime? MatchedAt { get; set; } = DateTime.UtcNow;
        public bool? IsAccepted { get; set; }
        public MatchStatus Status { get; set; } = MatchStatus.Pending;
        public int? ListingId { get; set; }
        //public Listing Listing { get; set; }
        public string UserId { get; set; } // who requested/applied
        public ApplicationUser User { get; set; }
        public string MatchedUserId { get; set; } // the other user
        public ApplicationUser MatchedUser { get; set; }
    }
}
