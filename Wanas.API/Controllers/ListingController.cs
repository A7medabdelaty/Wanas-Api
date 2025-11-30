using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using Wanas.Application.DTOs.Listing;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;

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

        // get all listings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListingCardDto>>> GetAll()
        {
            var listings = await _listService.GetAllListingsAsync();
            if (listings == null || !listings.Any())
                return NotFound("No listings found.");
            return Ok(listings);
        }

        // get listing by id
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ListingDetailsDto>> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid id.");

            var listing = await _listService.GetListingByIdAsync(id);
            if (listing == null)
                return NotFound();

            return Ok(listing);
        }

        // create listing
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ListingDetailsDto>> Create([FromForm] CreateListingDto dto, [FromForm] string rooms)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();
            dto.Rooms = JsonConvert.DeserializeObject<List<CreateRoomDto>>(rooms);
            try
            {
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

        // update listing
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ListingDetailsDto>> Update(int id, [FromForm] UpdateListingDto dto, [FromForm] string rooms)
        {
            if (id <= 0)
                return BadRequest("Invalid id.");

            dto.Rooms = JsonConvert.DeserializeObject<List<UpdateRoomDto>>(rooms);

            try
            {
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

        // delete listing
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

        // get listings by user
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ListingCardDto>>> GetListingsByUser(string userId)
        {
            var listings = await _listService.GetListingsByUserAsync(userId);

            if (!listings.Any())
                return NotFound("No listings found for this user.");

            return Ok(listings);
        }

        // add photos
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

        // delete listing photo
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

        // get listing comments
        [HttpGet("{listingId:int}/comments")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(int listingId)
        {
            if (listingId <= 0)
                return BadRequest("Invalid id.");

            var comments = await _commentService.GetCommentsByListingAsync(listingId);
            return Ok(comments);
        }

        // add comment
        [Authorize]
        [HttpPost("{listingId:int}/comments")]
        public async Task<ActionResult<CommentDto>> CreateComment(int listingId, [FromBody] CreateCommentDto dto)
        {
            dto.ListingId = listingId;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = await _commentService.CreateCommentAsync(dto, userId);

            return CreatedAtAction(nameof(GetComments),
                new { listingId },
                comment);
        }

        // update comment
        [Authorize]
        [HttpPut("{listingId:int}/comments/{commentId:int}")]
        public async Task<ActionResult<CommentDto>> UpdateComment(int listingId, int commentId, [FromBody] UpdateCommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var updated = await _commentService.UpdateCommentAsync(commentId, dto, userId);

            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        // get comment
        [HttpGet("{listingId:int}/comments/{commentId:int}")]
        public async Task<ActionResult<CommentDto>> GetComment(int listingId, int commentId)
        {
            var comment = await _commentService.GetCommentWithRepliesAsync(commentId);

            if (comment == null)
                return NotFound();

            return Ok(comment);
        }
    }
}
