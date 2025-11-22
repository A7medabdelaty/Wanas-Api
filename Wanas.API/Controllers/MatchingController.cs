using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wanas.Application.Interfaces;

namespace Wanas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "VerifiedUser")]
    public class MatchingController : ControllerBase
    {
        private readonly IMatchingService _matchingService;

        public MatchingController(IMatchingService matchingService)
        {
            _matchingService = matchingService;
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> MatchUser(string id)
        {
            var results = await _matchingService.MatchUserAsync(id);
            return Ok(results);
        }
    }
}
