using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wanas.API.Responses;
using Wanas.Application.DTOs.Booking;
using Wanas.Application.DTOs.Payment;
using Wanas.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBedReservationService _reservationService;

    public BookingsController(IBedReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Reserve Beds
    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveBeds([FromBody] ReserveBedsRequestDto dto)
    {
        var userId = GetUserId();
        var res = await _reservationService.ReserveBedsAsync(userId, dto);
        return Ok(new ApiResponse(res));
    }

    // Owner approves payment
    [HttpPost("{reservationId}/approve-payment")]
    public async Task<IActionResult> ApprovePayment(int reservationId)
    {
        var ownerId = GetUserId();
        var ok = await _reservationService.ApprovePaymentAsync(reservationId, ownerId);
        return ok ? Ok(ApiResponse.Ok(null, "Payment approved"))
                  : BadRequest(new ApiError("ApprovalFailed"));
    }

    // User pays deposit
    [HttpPost("pay-deposit")]
    public async Task<IActionResult> PayDeposit([FromBody] DepositPaymentRequestDto dto)
    {
        var userId = GetUserId();
        var result = await _reservationService.PayDepositAsync(dto.ReservationId, userId);
        return Ok(new ApiResponse(result));
    }

    // Owner final confirmation
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmReservationRequestDto dto)
    {
        var ownerId = GetUserId();
        var ok = await _reservationService.ConfirmReservationAsync(ownerId, dto);
        return ok ? Ok(ApiResponse.Ok(null, "Reservation confirmed"))
                  : BadRequest(new ApiError("ConfirmFailed"));
    }

    // Cancel
    [HttpPost("cancel/{reservationId}")]
    public async Task<IActionResult> Cancel(int reservationId)
    {
        var userId = GetUserId();
        var ok = await _reservationService.CancelReservationAsync(userId, reservationId);
        return ok ? Ok(ApiResponse.Ok(null, "Reservation cancelled"))
                  : BadRequest(new ApiError("CancelFailed"));
    }
}
