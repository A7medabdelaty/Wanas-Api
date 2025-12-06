using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wanas.Application.Interfaces;

namespace Wanas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MatchingController : ControllerBase
    {
        private readonly IMatchingService _matchingService;
        private readonly IRoommateMatchingService _roommateMatchingService;

        public MatchingController(IMatchingService matchingService, IRoommateMatchingService roommateMatchingService)
        {
            _matchingService = matchingService;
            _roommateMatchingService = roommateMatchingService;
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> MatchUser(string id)
        {
            var results = await _matchingService.MatchUserAsync(id);
            return Ok(results);
        }

        [HttpGet("roommates/{id}")]
        public async Task<IActionResult> MatchRoommates(string id, [FromQuery] int top = 10)
        {
            var results = await _roommateMatchingService.MatchRoommatesAsync(id, top);
            return Ok(results);
        }
    }
}
