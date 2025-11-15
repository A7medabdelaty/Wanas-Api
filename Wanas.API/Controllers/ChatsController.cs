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

        // Get all chats for a specific user
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserChats(string userId)
        {
            var chats = await _chatService.GetUserChatsAsync(userId);
            return Ok(chats);
        }

        // Get full chat details with messages
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChatDetails(int id)
        {
            var chat = await _chatService.GetChatDetailsAsync(id);
            if (chat == null)
                return NotFound("Chat not found.");

            return Ok(chat);
        }

        // Create a new chat
        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var chat = await _chatService.CreateChatAsync(request);
            return CreatedAtAction(nameof(GetChatDetails), new { id = chat.Id }, chat);
        }

        // Update chat (rename or toggle group)
        [HttpPut("{chatId}")]
        public async Task<IActionResult> UpdateChat(int chatId, [FromBody] UpdateChatRequestDto request)
        {
            var chat = await _chatService.UpdateChatAsync(chatId, request);
            if (chat == null)
                return NotFound("Chat not found.");

            return Ok(chat);
        }

        // Delete chat
        [HttpDelete("{chatId}")]
        public async Task<IActionResult> DeleteChat(int chatId)
        {
            var success = await _chatService.DeleteChatAsync(chatId);
            if (!success)
                return NotFound("Chat not found.");
            return Ok("Chat deleted successfully.");
        }

        // Add participant
        [HttpPost("{chatId}/participants")]
        public async Task<IActionResult> AddParticipant(int chatId, [FromBody] AddParticipantRequestDto request)
        {
            request.ChatId = chatId;
            var result = await _chatService.AddParticipantAsync(request);

            if (!result)
                return BadRequest("User already in chat or chat not found.");

            return Ok("Participant added successfully.");
        }

        // Remove participant
        [HttpDelete("{chatId}/participants/{userId}")]
        public async Task<IActionResult> RemoveParticipant(int chatId, string userId)
        {
            var result = await _chatService.RemoveParticipantAsync(chatId, userId);
            if (!result)
                return NotFound("Participant not found or already removed.");

            return Ok("Participant removed successfully.");
        }

        // Leave chat (for user)
        [HttpDelete("{chatId}/leave/{userId}")]
        public async Task<IActionResult> LeaveChat(int chatId, string userId)
        {
            var result = await _chatService.LeaveChatAsync(chatId, userId);
            if (!result)
                return NotFound("User not found in chat.");

            return Ok("Left chat successfully.");
        }

        // Mark all messages in chat as read
        [HttpPost("{chatId}/mark-read/{userId}")]
        public async Task<IActionResult> MarkChatAsRead(int chatId, string userId)
        {
            var result = await _chatService.MarkChatAsReadAsync(chatId, userId);
            if (!result)
                return NotFound("Chat not found.");

            return Ok("Messages marked as read.");
        }

        // Get count of unread messages for user
        [HttpGet("user/{userId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(string userId)
        {
            var count = await _chatService.GetUnreadMessagesCountAsync(userId);
            return Ok(new { UnreadCount = count });
        }

        // Get recent chats summary for user
        [HttpGet("user/{userId}/recent")]
        public async Task<IActionResult> GetRecentChats(string userId)
        {
            var chats = await _chatService.GetRecentChatsAsync(userId);
            return Ok(chats);
        }
    }
}
