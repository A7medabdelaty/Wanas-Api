using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wanas.API.Responses;
using Wanas.Application.DTOs.Approval;
using Wanas.Application.Interfaces;

namespace Wanas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingApprovalController : ControllerBase
    {
        private readonly IBookingApprovalService _approvalService;
        private readonly IChatService _chatService;
        private readonly IListingService _listingService;

        public BookingApprovalController(
            IBookingApprovalService approvalService,
            IChatService chatService,
            IListingService listingService)
        {
            _approvalService = approvalService;
            _chatService = chatService;
            _listingService = listingService;
        }

        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 1) APPROVE USER → GROUP CHAT (Owner action)
        [HttpPost("{listingId}/approve-to-group/{userId}")]
        public async Task<IActionResult> ApproveToGroup(int listingId, string userId)
        {
            var ownerId = GetUserId();
            if (ownerId == null)
                return Unauthorized();

            // validate listing belongs to this owner
            var listing = await _listingService.GetListingByIdAsync(listingId);
            if (listing == null)
                return NotFound(new ApiError("ListingNotFound"));

            if (listing.OwnerId != ownerId)
                return Forbid();

            // ensure PRIVATE chat exists between owner & user for THIS listing
            var chatDto = await _chatService.GetOrCreatePrivateChatByListingAsync(ownerId, userId, listingId);

            // approve to GROUP chat (creates record + notify)
            var success = await _approvalService.ApproveToGroupAsync(listingId, ownerId, userId);

            if (!success)
                return BadRequest(new ApiError("GroupAddFailed", "User already approved or added"));

            return Ok(ApiResponse.Ok(null, "User successfully added to group chat."));
        }


        // 2) APPROVE PAYMENT (Owner action)
        [HttpPost("approve-payment")]
        public async Task<IActionResult> ApprovePayment([FromBody] PaymentApprovalRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ownerId = GetUserId();
            if (ownerId == null)
                return Unauthorized();

            // ensure listing belongs to current owner
            var listing = await _listingService.GetListingByIdAsync(dto.ListingId);
            if (listing == null)
                return NotFound(new ApiError("ListingNotFound"));

            if (listing.OwnerId != ownerId)
                return Forbid();

            // ensure private chat exists (for messaging + notifications)
            var chatDto = await _chatService.GetOrCreatePrivateChatByListingAsync(ownerId, dto.UserId, dto.ListingId);

            // approve payment
            var ok = await _approvalService.ApprovePaymentAsync(dto.ListingId, ownerId, dto.UserId);

            if (!ok)
                return BadRequest(new ApiError("PaymentApprovalFailed", "Payment already approved"));

            return Ok(ApiResponse.Ok(null, "Payment approved successfully."));
        }

        [HttpGet("status/{listingId:int}/{userId}")]
        public async Task<IActionResult> GetStatus(int listingId, string userId)
        {
            var requesterId = GetUserId();
            if (requesterId == null)
                return Unauthorized();

            var status = await _approvalService.GetApprovalStatusAsync(listingId, userId, requesterId);

            if (status == null)
                return NotFound(new ApiError("ListingNotFound"));

            return Ok(new ApiResponse(status));
        }

    }
}