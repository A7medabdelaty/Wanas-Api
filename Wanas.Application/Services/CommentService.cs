using AutoMapper;
using Wanas.Application.DTOs.Listing;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork unit;
        private readonly IMapper mapper;

        public CommentService(IUnitOfWork unit, IMapper mapper)
        {
            this.unit = unit;
            this.mapper = mapper;
        }

        public async Task<CommentDto> CreateCommentAsync(CreateCommentDto dto, string userId)
        {
            var comment = mapper.Map<Comment>(dto);
            comment.AuthorId = userId;
            comment.CreatedAt = DateTime.UtcNow;

            await unit.Comments.AddAsync(comment);
            await unit.CommitAsync();

            var commentWithAuthor = await unit.Comments.GetCommentWithAuthorAndRepliesAsync(comment.Id);

            return mapper.Map<CommentDto>(commentWithAuthor);
        }

        public async Task<CommentDto?> UpdateCommentAsync(int commentId, UpdateCommentDto dto, string userId)
        {
            var comment = await unit.Comments.GetByIdAsync(commentId);
            if (comment == null || comment.AuthorId != userId)
                return null;

            mapper.Map(dto, comment);

            unit.Comments.Update(comment);
            await unit.CommitAsync();

            return mapper.Map<CommentDto>(comment);
        }

        public async Task<bool> DeleteCommentAsync(int commentId, string userId)
        {
            var comment = await unit.Comments.GetByIdAsync(commentId);
            if (comment == null || comment.AuthorId != userId)
                return false;

            unit.Comments.Remove(comment);
            await unit.CommitAsync();

            return true;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByListingAsync(int listingId)
        {
            var comments = await unit.Comments.GetCommentsByListingAsync(listingId);
            return mapper.Map<IEnumerable<CommentDto>>(comments);
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByUserAsync(string userId)
        {
            var comments = await unit.Comments.GetCommentsByUserAsync(userId);
            return mapper.Map<IEnumerable<CommentDto>>(comments);
        }

        public async Task<CommentDto?> GetCommentWithRepliesAsync(int commentId)
        {
            var comment = await unit.Comments.GetCommentWithAuthorAndRepliesAsync(commentId);
            return comment == null ? null : mapper.Map<CommentDto>(comment);
        }
    }


}
