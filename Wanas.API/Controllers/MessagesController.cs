using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wanas.Application.DTOs.Message;
using Wanas.Application.Interfaces;

namespace Wanas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        // GET: api/messages/chat/{chatId}?limit=50
        [HttpGet("chat/{chatId}")]
        public async Task<IActionResult> GetMessagesByChat(int chatId, [FromQuery] int limit = 50)
        {
            var messages = await _messageService.GetMessagesByChatAsync(chatId, limit);
            return Ok(messages);
        }

        // POST: api/messages
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] CreateMessageRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var message = await _messageService.SendMessageAsync(request);
            return Ok(message);
        }
    }
}
