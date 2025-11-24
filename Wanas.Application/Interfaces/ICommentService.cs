using Wanas.Application.DTOs.Listing;

namespace Wanas.Application.Interfaces
{
    public interface ICommentService
    {
        Task<CommentDto> CreateCommentAsync(CreateCommentDto dto, string userId);
        Task<CommentDto?> UpdateCommentAsync(int commentId, UpdateCommentDto dto, string userId);
        Task<bool> DeleteCommentAsync(int commentId, string userId);
        Task<IEnumerable<CommentDto>> GetCommentsByListingAsync(int listingId);
        Task<IEnumerable<CommentDto>> GetCommentsByUserAsync(string userId);
        Task<CommentDto?> GetCommentWithRepliesAsync(int commentId);
    }


}
