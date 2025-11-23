using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wanas.Application.DTOs.AI;
using Wanas.Application.Interfaces.AI;

namespace Wanas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        //private readonly IGenerateListingService _generateListingService;
        private readonly IListingDescriptionService _listingDescriptionService;
        public AIController(IListingDescriptionService listingDescriptionService )
        {
            _listingDescriptionService = listingDescriptionService;
        }

        ///// <summary>
        ///// Generate listing suggestions using AI
        ///// </summary>
        ///// 
        //[HttpPost("generate-listing")]
        //[Authorize]
        //public async Task<IActionResult> GenerateListing([FromBody] GenerateListingRequestDto request)
        //{
        //    var userId = User.Claims.("id")?.Value;

        //    if (userId == null) return Unauthorized();

        //    var response = await _generateListingService.GenerateListingAsync(request, userId);

        //    return Ok(response);
        //}

        [HttpPost("generate-description")]
        [Authorize]
        public async Task<IActionResult> GenerateDescription([FromBody] GenerateDescriptionDto dto)
        {
            var text = await _listingDescriptionService.GenerateDescriptionAsync(dto);
            return Ok(text);
        }

    }
}
