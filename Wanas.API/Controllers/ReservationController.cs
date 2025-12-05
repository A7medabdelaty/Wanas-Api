using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wanas.API.Responses;
using Wanas.Application.DTOs.Payment;
using Wanas.Application.DTOs.Reservation;
using Wanas.Application.Interfaces;
using Wanas.Domain.Enums;

namespace Wanas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService service)
        {
            _reservationService = service;
        }
        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpPost]
        public async Task<IActionResult> CreateReservation(
            [FromBody] CreateReservationRequestDto request)
        {
            var userId = GetUserId();

            if (request == null)
                return BadRequest(new ApiError("InvalidRequest"));

            if (request.StartDate == default)
                return BadRequest(new ApiError("InvalidStartDate"));

            if (request.DurationInDays != 15 && request.DurationInDays != 30)
                return BadRequest(new ApiError("InvalidDuration", "Duration must be 15 or 30 days"));

            try
            {
                var reservation = await _reservationService
                    .CreateReservationAsync(request, userId);

                return Created(
                    $"/api/reservations/{reservation.Id}",
                    reservation
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiError(ex.Message));
            }
        }

        [HttpPost("{reservationId:int}/deposit")]
        public async Task<IActionResult> PayDeposit(
            int reservationId,
            [FromBody] DepositPaymentRequestDto payment)
        {
            if (payment == null)
                return BadRequest(new ApiError("InvalidPaymentData"));

            var userId = GetUserId();

            try
            {
                var result = await _reservationService
                    .PayDepositAsync(reservationId, payment, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Forbidden")
                    return Forbid();

                if (ex.Message == "ReservationNotFound")
                    return NotFound(new ApiError("ReservationNotFound"));

                if (ex.Message == "AlreadyPaidOrInvalidState")
                    return BadRequest(new ApiError("ReservationAlreadyPaid"));

                if (ex.Message == "ReservationExpired")
                    return BadRequest(new ApiError("ReservationExpired"));

                return BadRequest(new ApiError(ex.Message));
            }
        }

        [HttpGet("owner")]
        public async Task<IActionResult> GetOwnerReservations()
        {
            var ownerId = GetUserId();
            if (ownerId == null)
                return Unauthorized();
            var result = await _reservationService.GetOwnerReservationsAsync(ownerId);
            if(result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyReservations()
        {
            var userId = GetUserId();

            var result = await _reservationService.GetRenterReservationsAsync(userId);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _reservationService.GetReservationAsync(id);
            return Ok(result);
        }
    }

}
