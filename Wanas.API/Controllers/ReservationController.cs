using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        public async Task<IActionResult> Create([FromBody] CreateReservationRequestDto request)
        {
            var userId = GetUserId();
            var result = await _reservationService.CreateReservationAsync(request, userId);
            return Ok(result);
        }

        [HttpPost("{reservationId:int}/deposit")]
        public async Task<IActionResult> PayDeposit(
        int reservationId,
        [FromBody] DepositPaymentRequestDto payment,
        CancellationToken cancellationToken)
        {
            if (payment == null)
                return BadRequest("InvalidPaymentData");

            var userId = GetUserId();
            var result = await _reservationService.PayDepositAsync(reservationId, payment, userId);

            if (result.PaymentStatus != PaymentStatus.Sucess)
                return BadRequest();

            return Ok(result);
        }


        [HttpPost("{id:int}/confirm-deposit")]
        public async Task<IActionResult> ConfirmDeposit(int id)
        {
            var userId = GetUserId();
            var result = await _reservationService.ConfirmDepositAsync(id, userId);
            return Ok(result);
        }

        [HttpGet("owner")]
        public async Task<IActionResult> GetOwnerReservations()
        {
            var ownerId = GetUserId();
            var result = await _reservationService.GetOwnerReservationsAsync(ownerId);
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
