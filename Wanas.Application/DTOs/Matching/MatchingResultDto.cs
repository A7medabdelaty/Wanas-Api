using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanas.Application.DTOs.Matching
{
    public class MatchingResultDto
    {
        public int ListingId { get; set; }
        public string ListingTitle { get; set; }
        public string ListingDescription { get; set; }
        public string ListingCity { get; set; }
        public int Price { get; set; }
        public string FirstPhotoUrl { get; set; }
        public int Score { get; set; }
    }
}
