using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wanas.API.Responses;
using Wanas.Application.DTOs.Chat;
using Wanas.Application.Interfaces;

namespace Wanas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatsController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatsController(IChatService chatService)
        {
            _chatService = chatService;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Get all chats for a specific user
        [HttpGet("user")]
        public async Task<IActionResult> GetUserChats()
        {
            var userId = GetUserId();
            var chats = await _chatService.GetUserChatsAsync(userId);
            return Ok(new ApiResponse(chats));
        }

        // Get full chat details with messages
        [HttpGet("{chatId:int}")]
        public async Task<IActionResult> GetChatDetails(int chatId)
        {
            if (chatId <= 0)
                return BadRequest(new ApiError("InvalidChatId"));

            var chat = await _chatService.GetChatDetailsAsync(chatId);
            if (chat == null)
                return NotFound(new ApiError("ChatNotFound"));

            return Ok(new ApiResponse(chat));
        }

        // Create a new chat
        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var chat = await _chatService.CreateChatAsync(request);
            return CreatedAtAction(nameof(GetChatDetails), new { chatId = chat.Id }, new ApiResponse(chat));
        }

        // Update chat (rename or toggle group)
        [HttpPut("{chatId:int}")]
        public async Task<IActionResult> UpdateChat(int chatId, [FromBody] UpdateChatRequestDto request)
        {
            var chat = await _chatService.UpdateChatAsync(chatId, request);
            if (chat == null)
                return NotFound("Chat not found.");

            return Ok(chat);
        }

        // Delete chat
        [HttpDelete("{chatId:int}")]
        public async Task<IActionResult> DeleteChat(int chatId)
        {
            var success = await _chatService.DeleteChatAsync(chatId);
            if (!success)
                return NotFound("Chat not found.");
            return Ok("Chat deleted successfully.");
        }

        // Add participant
        [HttpPost("{chatId:int}/participants")]
        public async Task<IActionResult> AddParticipant(int chatId, [FromBody] AddParticipantRequestDto request)
        {
            if (chatId <= 0)
                return BadRequest(new ApiError("InvalidChatId"));

            var result = await _chatService.AddParticipantAsync(chatId, request.UserId);

            if (!result)
                return BadRequest(new ApiError("ParticipantAddFailed", "User already in chat or chat not found"));

            return Ok(ApiResponse.Ok(null, "Participant added successfully"));
        }

        // Remove participant
        [HttpDelete("{chatId:int}/participants/{userId}")]
        public async Task<IActionResult> RemoveParticipant(int chatId, string userId)
        {
            var result = await _chatService.RemoveParticipantAsync(chatId, userId);
            if (!result)
                return NotFound(new ApiError("ParticipantNotFound"));

            return Ok(ApiResponse.Ok(null, "Participant removed successfully"));
        }

        // Leave chat (for user)
        [HttpDelete("{chatId:int}/leave")]
        public async Task<IActionResult> LeaveChat(int chatId)
        {
            var userId = GetUserId();

            var result = await _chatService.LeaveChatAsync(chatId, userId);
            if (!result)
                return NotFound(new ApiError("UserNotInChat"));

            return Ok(ApiResponse.Ok(null, "Successfully left the chat"));
        }

        // Mark all messages in chat as read
        [HttpPost("{chatId:int}/mark-read")]
        public async Task<IActionResult> MarkChatAsRead(int chatId)
        {
            var userId = GetUserId();

            var result = await _chatService.MarkChatAsReadAsync(chatId, userId);
            if (!result)
                return NotFound(new ApiError("ChatNotFound"));

            return Ok(ApiResponse.Ok(null, "Messages marked as read"));
        }

        // Get count of unread messages for user
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetUserId();

            var count = await _chatService.GetUnreadMessagesCountAsync(userId);
            return Ok(ApiResponse.Ok(new { UnreadCount = count }));
        }

        // Get recent chats summary for user
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentChats()
        {
            var userId = GetUserId();

            var chats = await _chatService.GetRecentChatsAsync(userId);
            return Ok(ApiResponse.Ok(chats));
        }
    }
}
