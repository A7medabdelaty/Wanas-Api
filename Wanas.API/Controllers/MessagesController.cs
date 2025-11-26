using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using Wanas.API.Responses;
using Wanas.Application.DTOs.Message;
using Wanas.Application.Interfaces;

namespace Wanas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "VerifiedUser")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        private string GetUserId()=> User.FindFirst(ClaimTypes.NameIdentifier)?.Value
       ?? throw new Exception("Invalid token: userId not found");

        // Get messages by chat ID
        [HttpGet("chat/{chatId:int}")]
        public async Task<IActionResult> GetMessagesByChat(int chatId, [FromQuery] int limit = 50)
        {
            if (chatId <= 0)
                return BadRequest(new ApiError("InvalidChatId"));

            var messages = await _messageService.GetMessagesByChatAsync(chatId, limit);
            return Ok(ApiResponse.Ok(messages));
        }

        // Send message
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] CreateMessageRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiError("ValidationError", "Invalid data sent"));

            request.SenderId = GetUserId();

            var message = await _messageService.SendMessageAsync(request);
            return Ok(ApiResponse.Ok(message, "Message sent successfully"));
        }

        // Edit message
        [HttpPut("{messageId:int}")]
        public async Task<IActionResult> EditMessage(int messageId, [FromBody] EditMessageRequestDto dto)
        {
            if (messageId <= 0)
                return BadRequest(new ApiError("InvalidMessageId"));

            var userId = GetUserId();

            var success = await _messageService.EditMessageAsync(
                messageId, dto.NewContent, userId);

            if (!success)
                return NotFound(new ApiError("MessageNotFoundOrForbidden"));

            return Ok(ApiResponse.Ok(null, "Message edited successfully"));
        }

        // Delete message
        [HttpDelete("{messageId:int}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            if (messageId <= 0)
                return BadRequest(new ApiError("InvalidMessageId"));

            var userId = GetUserId();

            var success = await _messageService.DeleteMessageAsync(messageId, userId);

            if (!success)
                return NotFound(new ApiError("MessageNotFoundOrForbidden"));

            return Ok(ApiResponse.Ok(null, "Message deleted successfully"));
        }

        // Mark a specific message as read
        [HttpPost("{messageId}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int messageId)
        {
            if (messageId <= 0)
                return BadRequest(new ApiError("InvalidMessageId"));

            var userId = GetUserId();

            var success = await _messageService.MarkMessageAsReadAsync(messageId, userId);

            if (!success)
                return NotFound(new ApiError("MessageNotFound"));

            return Ok(ApiResponse.Ok(null, "Message marked as read"));
        }
    }
}
