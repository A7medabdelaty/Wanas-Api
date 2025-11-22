using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wanas.Application.Interfaces;

namespace Wanas.API.Controllers
{
    [ApiController]
    [Route("api/admin/analytics")]
    [Authorize(Roles = "Admin")]
    public class AdminAnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analytics;
        public AdminAnalyticsController(IAnalyticsService analytics) { _analytics = analytics; }
        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromQuery] DateOnly? date = null)
        {
            var d = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var dto = await _analytics.GetSummaryAsync(d);
            return Ok(dto);
        }
        [HttpGet("traffic")]
        public async Task<IActionResult> Traffic([FromQuery] DateOnly from, [FromQuery] DateOnly to)
        {
            var points = await _analytics.GetTrafficAsync(from, to);
            return Ok(points);
        }
        [HttpGet("moderation")]
        public async Task<IActionResult> Moderation([FromQuery] DateOnly? date = null)
        {
            var d = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var dto = await _analytics.GetModerationKpisAsync(d);
            return Ok(dto);
        }
    }
}