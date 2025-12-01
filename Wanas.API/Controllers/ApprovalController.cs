using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wanas.API.Responses;
using Wanas.Application.Interfaces;

[Authorize]
[ApiController]
[Route("api/listings")]
public class ApprovalController : ControllerBase
{
    private readonly IBookingApprovalService _approvalService;

    public ApprovalController(IBookingApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    [HttpPost("{listingId}/approve-payment/{userId}")]
    public async Task<IActionResult> ApprovePayment(int listingId, string userId)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (ownerId == null)
            return Unauthorized(ownerId);

        var ok = await _approvalService.ApprovePaymentAsync(listingId, ownerId, userId);
        if (!ok)
            return BadRequest(new ApiError("ApprovalFailed"));

        return Ok(new ApiResponse("Payment approved successfully."));
    }

    [HttpPost("{listingId}/approve-to-group/{userId}")]
    public async Task<IActionResult> ApproveToGroup(int listingId, string userId)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (ownerId == null)
            return Unauthorized(ownerId);

        var ok = await _approvalService.ApproveToGroupAsync(listingId, ownerId, userId);
        if (!ok)
            return BadRequest(new ApiError("GroupAddFailed"));

        return Ok(new ApiResponse("User added to group chat."));
    }
}
