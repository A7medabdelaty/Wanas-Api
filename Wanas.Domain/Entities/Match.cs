using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    public class Match
    {
        public int MatchId { get; set; }
        public double MatchScore { get; set; }
        public DateTime MatchedAt { get; set; }
        public MatchStatus Status { get; set; } = MatchStatus.Pending;
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int ListingId { get; set; }
        //public Listing Listing { get; set; }
    }
}
