using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wanas.Application.DTOs.Booking;
using Wanas.Application.Interfaces;
using Wanas.API.Responses;

namespace Wanas.API.Controllers
{
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

        // POST: /api/bookings/reserve
        [HttpPost("reserve")]
        public async Task<IActionResult> ReserveBeds([FromBody] ReserveBedsRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                var res = await _reservationService.ReserveBedsAsync(userId, dto);
                return Ok(new ApiResponse(res));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiError("ReservationFailed", ex.Message));
            }
        }

        // POST: /api/bookings/confirm
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmReservation([FromBody] ConfirmReservationRequestDto dto)
        {
            var ownerId = GetUserId();
            if (ownerId == null)
                return Unauthorized();

            var ok = await _reservationService.ConfirmReservationAsync(ownerId, dto);
            if (!ok)
                return BadRequest(new ApiError("ConfirmFailed"));

            return Ok(ApiResponse.Ok(null, "Reservation confirmed."));
        }

        // POST: /api/bookings/cancel/{reservationId}
        [HttpPost("cancel/{reservationId:int}")]
        public async Task<IActionResult> CancelReservation(int reservationId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var ok = await _reservationService.CancelReservationAsync(userId, reservationId);
            if (!ok)
                return BadRequest(new ApiError("CancelFailed"));

            return Ok(ApiResponse.Ok(null, "Reservation cancelled."));
        }

        // GET: /api/bookings/{reservationId}
        [HttpGet("{reservationId:int}")]
        public async Task<IActionResult> GetReservation(int reservationId)
        {
            var res = await _reservationService.GetReservationAsync(reservationId);
            if (res == null)
                return NotFound(new ApiError("ReservationNotFound"));
            return Ok(new ApiResponse(res));
        }
    }
}
