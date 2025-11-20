
using Microsoft.AspNetCore.Mvc;
using Wanas.Application.DTOs.Chatbot;
using Wanas.Application.Interfaces.AI;

namespace Wanas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {

        private readonly IChatbotService _chatbotService;
        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] ChatbotRequestDto request)
        {
            var response = await _chatbotService.SendMessageAsync(request);
            return Ok(response);
        }
    }
}
