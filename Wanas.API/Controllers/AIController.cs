//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Wanas.Application.DTOs.AI;
//using Wanas.Application.Interfaces.AI;

//namespace Wanas.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AIController : ControllerBase
//    {
//        private readonly IGenerateListingService _generateListingService;

//        public AIController(IGenerateListingService generateListingService)
//        {
//            _generateListingService = generateListingService;

//        }

//        /// <summary>
//        /// Generate listing suggestions using AI
//        /// </summary>
//        /// 
//        [HttpPost("generate-listing")]
//        [Authorize]
//        public async Task<IActionResult> GenerateListing([FromBody] GenerateListingRequestDto request)
//        {
//            var userId = User.Claims.("id")?.Value;

//            if (userId == null) return Unauthorized();  

//            var response = await _generateListingService.GenerateListingAsync(request,userId);

//            return Ok(response);
//        }
//    }
//}
