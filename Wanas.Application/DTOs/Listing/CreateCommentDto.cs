
namespace Wanas.Application.DTOs.Listing
{
    public class CreateCommentDto
    {
        public string Content { get; set; }
        public int ListingId { get; set; }
        public int? ParentCommentId { get; set; }
    }

}
