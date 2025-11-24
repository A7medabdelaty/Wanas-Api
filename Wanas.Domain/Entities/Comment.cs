
using System.Text.Json.Serialization;

namespace Wanas.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // public DateTime? DeletedAt { get; set; }

        public string AuthorId { get; set; }
        public virtual ApplicationUser Author { get; set; }
        public int ListingId { get; set; }
        public virtual Listing Listing { get; set; }

        public int? ParentCommentId { get; set; }
        [JsonIgnore]
        public virtual Comment? ParentComment { get; set; }

        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
