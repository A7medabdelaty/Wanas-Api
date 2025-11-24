using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wanas.Application.DTOs.Listing;
using Wanas.Application.Interfaces;
using Wanas.Application.Services;

namespace Wanas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingController : ControllerBase
    {
        private readonly IListingService listServ;
        private readonly ICommentService comServ;

        public ListingController(IListingService listServ, ICommentService comServ)
        {
            this.listServ = listServ;
            this.comServ = comServ;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListingDetailsDto>>> GetAll()
        {
            var listings = await listServ.GetAllListingsAsync();
            return Ok(listings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ListingDetailsDto>> GetById(int id)
        {
            var listing = await listServ.GetListingByIdAsync(id);
            if (listing == null) return NotFound();
            return Ok(listing);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ListingDetailsDto>> Create([FromForm] CreateListingDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var listing = await listServ.CreateListingAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = listing.Id }, listing);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ListingDetailsDto>> Update(int id, [FromForm] UpdateListingDto dto)
        {
            var updated = await listServ.UpdateListingAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await listServ.DeleteListingAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [Authorize]
        [HttpPost("{id}/photos")]
        public async Task<IActionResult> AddPhotos(int id, [FromForm] List<IFormFile> photos)
        {
            await listServ.AddPhotosToListingAsync(id, photos);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{listingId}/photos/{photoId}")]
        public async Task<IActionResult> DeletePhoto(int listingId, int photoId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await listServ.RemovePhotoAsync(listingId, photoId, userId);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpGet("{listingId}/comments")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(int listingId)
        {
            var comments = await comServ.GetCommentsByListingAsync(listingId);
            return Ok(comments);
        }

        [Authorize]
        [HttpPost("{listingId}/comments")]
        public async Task<ActionResult<CommentDto>> CreateComment(int listingId, [FromBody] CreateCommentDto dto)
        {
            dto.ListingId = listingId;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = await comServ.CreateCommentAsync(dto, userId);
            return CreatedAtAction(nameof(GetComments), new { listingId }, comment);
        }

        [Authorize]
        [HttpPut("{listingId}/comments/{commentId}")]
        public async Task<ActionResult<CommentDto>> UpdateComment(int listingId, int commentId, [FromBody] UpdateCommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var updated = await comServ.UpdateCommentAsync(commentId, dto, userId);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpGet("{listingId}/comments/{commentId}")]
        public async Task<ActionResult<CommentDto>> GetComment(int listingId, int commentId)
        {
            var comment = await comServ.GetCommentWithRepliesAsync(commentId);
            if (comment == null) return NotFound();
            return Ok(comment);
        }
    }
}
