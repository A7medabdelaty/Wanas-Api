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

        // ✅ Get messages by chat ID
        [HttpGet("chat/{chatId}")]
        public async Task<IActionResult> GetMessagesByChat(int chatId, [FromQuery] int limit = 50)
        {
            var messages = await _messageService.GetMessagesByChatAsync(chatId, limit);
            return Ok(messages);
        }

        // ✅ Send message
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] CreateMessageRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var message = await _messageService.SendMessageAsync(request);
            return Ok(message);
        }

        // ✅ Edit message
        [HttpPut("{messageId}")]
        public async Task<IActionResult> EditMessage(int messageId, [FromBody] string newContent)
        {
            var success = await _messageService.EditMessageAsync(messageId, newContent);
            if (!success)
                return NotFound("Message not found.");

            return Ok("Message edited successfully.");
        }

        // ✅ Delete message
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var success = await _messageService.DeleteMessageAsync(messageId);
            if (!success)
                return NotFound("Message not found.");

            return Ok("Message deleted successfully.");
        }

        // ✅ Mark a specific message as read
        [HttpPost("{messageId}/read/{userId}")]
        public async Task<IActionResult> MarkMessageAsRead(int messageId, string userId)
        {
            var success = await _messageService.MarkMessageAsReadAsync(messageId, userId);
            if (!success)
                return NotFound("Message not found.");

            return Ok("Message marked as read.");
        }
    }
}
