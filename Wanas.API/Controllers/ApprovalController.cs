using Microsoft.AspNetCore.Mvc;
using Wanas.Application.DTOs.Approval;
using Wanas.Application.Interfaces;
namespace Wanas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingApprovalController : ControllerBase
    {
        private readonly IBookingApprovalService _approvalService;

        public BookingApprovalController(IBookingApprovalService approvalService)
        {
            _approvalService = approvalService;
        }

        [HttpPost("approve-group")]
        public async Task<IActionResult> ApproveGroup([FromBody] GroupApprovalRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ok = await _approvalService.ApproveToGroupAsync(dto.ListingId, dto.OwnerId, dto.UserId);

            return ok
                ? Ok(new { Message = "User added to group chat successfully." })
                : BadRequest(new { Error = "Group approval failed or already approved." });
        }

        [HttpPost("approve-payment")]
        public async Task<IActionResult> ApprovePayment([FromBody] PaymentApprovalRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ok = await _approvalService.ApprovePaymentAsync(dto.ListingId, dto.OwnerId, dto.UserId);

            return ok
                ? Ok(new { Message = "Payment approved successfully." })
                : BadRequest(new { Error = "Payment approval failed or already approved." });
        }
    }
}