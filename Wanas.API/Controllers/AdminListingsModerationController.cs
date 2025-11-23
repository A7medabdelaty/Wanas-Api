using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wanas.Application.DTOs.Listing;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;

namespace Wanas.API.Controllers
{
    [ApiController]
    [Route("api/admin/listings/moderation")]
    [Authorize(Roles = "Admin")] // adjust later for specific moderation roles
    public class AdminListingsModerationController : ControllerBase
    {
        private readonly IListingModerationService _moderationService;
        public AdminListingsModerationController(IListingModerationService moderationService)
        {
            _moderationService = moderationService;
        }
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var list = await _moderationService.GetPendingAsync();
            return Ok(new { totalCount = list.Count(), listings = list });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetState(int id)
        {
            var state = await _moderationService.GetModerationStateAsync(id);
            if (state == null) return NotFound(new { message = "Listing not found" });
            return Ok(state);
        }
        public class ModerateRequest { public ListingModerationStatus NewStatus { get; set; } public string? Note { get; set; } }
        [HttpPost("{id}/moderate")]
        public async Task<IActionResult> Moderate(int id, [FromBody] ModerateRequest req)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var ok = await _moderationService.ModerateAsync(id, req.NewStatus, adminId, req.Note);
            if (!ok) return NotFound(new { message = "Listing not found" });
            return Ok(new { message = "Listing moderated", listingId = id, status = req.NewStatus });
        }
        public class FlagRequest { public string Reason { get; set; } = string.Empty; }
        [HttpPost("{id}/flag")]
        public async Task<IActionResult> Flag(int id, [FromBody] FlagRequest req)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var ok = await _moderationService.FlagAsync(id, adminId, req.Reason);
            if (!ok) return NotFound(new { message = "Listing not found" });
            return Ok(new { message = "Listing Flag Changed", listingId = id });
        }

        #region previous flagging method

        //public class FlagRequest { public string Reason { get; set; } = string.Empty; }
        //[HttpPost("{id}/flag")]
        //public async Task<IActionResult> Flag(int id, [FromBody] FlagRequest req)
        //{
        //    var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        //    var ok = await _moderationService.FlagAsync(id, adminId, req.Reason);
        //    if (!ok) return NotFound(new { message = "Listing not found" });
        //    return Ok(new { message = "Listing flagged", listingId = id });
        //}
        //public class UnflagRequest { public string? Note { get; set; } }
        //[HttpPost("{id}/unflag")]
        //public async Task<IActionResult> Unflag(int id, [FromBody] UnflagRequest req)
        //{
        //    var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        //    var ok = await _moderationService.UnflagAsync(id, adminId, req.Note);
        //    if (!ok) return NotFound(new { message = "Listing not found" });
        //    return Ok(new { message = "Listing unflagged", listingId = id });
        //}
        #endregion
    }
}