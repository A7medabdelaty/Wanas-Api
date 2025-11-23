using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wanas.Application.Interfaces;

namespace Wanas.API.Controllers
{
    [ApiController]
    [Route("api/admin/revenue")]
    [Authorize(Roles = "Admin")]
    public class AdminRevenueController : ControllerBase
    {
        private readonly IRevenueService _revenue;
        public AdminRevenueController(IRevenueService revenue) { _revenue = revenue; }
        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (from == default || to == default) return BadRequest(new { message = "from/to required" });
            var dto = await _revenue.GetSummaryAsync(from, to);
            return Ok(dto);
        }
        [HttpGet("payments")]
        public async Task<IActionResult> Payments([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (from == default || to == default) return BadRequest(new { message = "from/to required" });
            var dtos = await _revenue.GetPaymentsAsync(from, to);
            return Ok(dtos);
        }
    }
}