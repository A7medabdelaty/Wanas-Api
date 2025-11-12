using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wanas.Application.DTOs.Chat;
using Wanas.Application.Interfaces;

namespace Wanas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class ChatsController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatsController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // GET: api/chats/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserChats(string userId)
        {
            var chats = await _chatService.GetUserChatsAsync(userId);
            return Ok(chats);
        }

        // GET: api/chats/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChatDetails(int id)
        {
            var chat = await _chatService.GetChatWithMessagesAsync(id);
            if (chat == null)
                return NotFound("Chat not found.");

            return Ok(chat);
        }

        // POST: api/chats
        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var chat = await _chatService.CreateChatAsync(request);
            return CreatedAtAction(nameof(GetChatDetails), new { id = chat.Id }, chat);
        }
    }
}
