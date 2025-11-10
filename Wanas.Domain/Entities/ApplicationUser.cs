
using Microsoft.AspNetCore.Identity;

namespace Wanas.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string City { get; set; }
        public string Bio { get; set; }
        public string LifeStyle { get; set; }
        public string UserRole { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Photo { get; set; }

        //public ICollection<Listing> Listings { get; set; }
        public ICollection<Match> Matches { get; set; }
    }
}
