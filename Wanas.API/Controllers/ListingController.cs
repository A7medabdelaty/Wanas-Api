using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using Wanas.Application.DTOs.Listing;
using Wanas.Application.Interfaces;
using Wanas.Domain.Enums;
using Wanas.Application.Responses;

namespace Wanas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingController : ControllerBase
    {
        private readonly IListingService _listService;
        private readonly ICommentService _commentService;
        private readonly ILogger<ListingController> _logger;

        public ListingController(
            IListingService listService,
            ICommentService commentService,
            ILogger<ListingController> logger)
        {
            _listService = listService;
            _commentService = commentService;
            _logger = logger;
        }

        // GET ALL (PAGED)
        [HttpGet]
        public async Task<ActionResult<ApiPagedResponse<ListingDetailsDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _listService.GetPagedListingsAsync(pageNumber, pageSize);
            return Ok(result);
        }

        // GET TOP6 ACTIVE APPROVED LISTINGS
        [HttpGet("top")]
        public async Task<ActionResult<IEnumerable<ListingDetailsDto>>> GetTop(int size = 6)
        {
            var active = await _listService.GetActiveListingsAsync();
            var top = active
                .Where(l => l.ModerationStatus == ListingModerationStatus.Approved)
                .OrderByDescending(l => l.CreatedAt)
                .Take(size)
                .ToList();

            if (!top.Any())
                return NotFound("No listings found.");

            return Ok(top);
        }

        // GET BY ID
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ListingDetailsDto>> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid id.");

            var listing = await _listService.GetListingByIdAsync(id);
            if (listing == null)
                return NotFound();
            if (listing.ModerationStatus != ListingModerationStatus.Approved)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                if (listing.OwnerId != userId && !isAdmin)
                    return NotFound();
            }

            return Ok(listing);
        }

        // CREATE LISTING
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ListingDetailsDto>> Create(
            [FromForm] CreateListingDto dto,
            [FromForm] string rooms)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(rooms))
                return BadRequest("Rooms JSON is required.");

            try
            {
                dto.Rooms = JsonConvert.DeserializeObject<List<CreateRoomDto>>(rooms)
                            ?? new List<CreateRoomDto>();

                var listing = await _listService.CreateListingAsync(dto, userId);

                return CreatedAtAction(nameof(GetById),
                    new { id = listing.Id },
                    listing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating listing.");
                return StatusCode(500, "An error occurred while creating the listing.");
            }
        }

        // UPDATE LISTING
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ListingDetailsDto>> Update(
            int id,
            [FromForm] UpdateListingDto dto,
            [FromForm] string rooms)
        {
            if (id <= 0)
                return BadRequest("Invalid id.");

            if (string.IsNullOrWhiteSpace(rooms))
                return BadRequest("Rooms JSON is required.");

            try
            {
                dto.Rooms = JsonConvert.DeserializeObject<List<UpdateRoomDto>>(rooms)
                            ?? new List<UpdateRoomDto>();

                var updated = await _listService.UpdateListingAsync(id, dto);

                if (updated == null)
                    return NotFound();

                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating listing {Id}", id);
                return StatusCode(500, "An error occurred while updating the listing.");
            }
        }

        // DELETE LISTING
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid id.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {User} requested deletion of listing {Id}", userId, id);

            try
            {
                var deleted = await _listService.DeleteListingAsync(id);

                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting listing {Id}", id);
                return StatusCode(500, "Internal server error while deleting listing.");
            }
        }

        // GET LISTINGS BY USER
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ListingCardDto>>> GetListingsByUser(string userId)
        {
            var listings = await _listService.GetListingsByUserAsync(userId);

            if (!listings.Any())
                return NotFound("No listings found for this user.");

            return Ok(listings);
        }

        // ADD PHOTOS
        [Authorize]
        [HttpPost("{id:int}/photos")]
        public async Task<IActionResult> AddPhotos(int id, [FromForm] List<IFormFile> photos)
        {
            if (id <= 0)
                return BadRequest("Invalid id.");

            if (photos == null || photos.Count == 0)
                return BadRequest("No photos supplied.");

            await _listService.AddPhotosToListingAsync(id, photos);
            return NoContent();
        }

        // DELETE PHOTO
        [Authorize]
        [HttpDelete("{listingId:int}/photos/{photoId:int}")]
        public async Task<IActionResult> DeletePhoto(int listingId, int photoId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (listingId <= 0 || photoId <= 0)
                return BadRequest("Invalid ids.");

            var success = await _listService.RemovePhotoAsync(listingId, photoId, userId);
            if (!success)
                return NotFound();

            return NoContent();
        }

        // REACTIVATE LISTING
        [Authorize]
        [HttpPost("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivateListing(int id)
        {
            try
            {
               var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
               var success = await _listService.ReactivateListingAsync(id, userId);
               if (success) return Ok(new { Message = "Listing reactivated" });
               return BadRequest(new { Message = "Cannot reactivate: no available beds." });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        // COMMENTS
        [HttpGet("{listingId:int}/comments")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(int listingId)
        {
            if (listingId <= 0)
                return BadRequest("Invalid id.");

            var comments = await _commentService.GetCommentsByListingAsync(listingId);
            return Ok(comments);
        }

        [Authorize]
        [HttpPost("{listingId:int}/comments")]
        public async Task<ActionResult<CommentDto>> CreateComment(
            int listingId,
            [FromBody] CreateCommentDto dto)
        {
            dto.ListingId = listingId;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = await _commentService.CreateCommentAsync(dto, userId);

            return CreatedAtAction(nameof(GetComments),
                new { listingId },
                comment);
        }

        [Authorize]
        [HttpPut("{listingId:int}/comments/{commentId:int}")]
        public async Task<ActionResult<CommentDto>> UpdateComment(
            int listingId,
            int commentId,
            [FromBody] UpdateCommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var updated = await _commentService.UpdateCommentAsync(commentId, dto, userId);

            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpGet("{listingId:int}/comments/{commentId:int}")]
        public async Task<ActionResult<CommentDto>> GetComment(int listingId, int commentId)
        {
            var comment = await _commentService.GetCommentWithRepliesAsync(commentId);

            if (comment == null)
                return NotFound();

            return Ok(comment);
        }

        [HttpDelete("{listingId:int}/comments/{commentId:int}")]
        public async Task<IActionResult> DeleteComment(int listingId, int commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deleted = await _commentService.DeleteCommentAsync(commentId, userId);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
