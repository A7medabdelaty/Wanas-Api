
namespace Wanas.Application.DTOs.Listing
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string AuthorName { get; set; }
        public string AuthorId { get; set; } 
        public string? AuthorPhoto { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<CommentDto> Replies { get; set; } = new List<CommentDto>();
    }
}
