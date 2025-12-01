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

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET: User Chats
        [HttpGet("user")]
        public async Task<IActionResult> GetUserChats()
        {
            var userId = GetUserId();

            var chats = await _chatService.GetUserChatsAsync(userId);
            return Ok(new ApiResponse(chats));
        }

        // GET: Chat Details (with messages)
        [HttpGet("{chatId:int}")]
        public async Task<IActionResult> GetChatDetails(int chatId)
        {
            if (chatId <= 0)
                return BadRequest(new ApiError("InvalidChatId"));

            var userId = GetUserId();
            var chat = await _chatService.GetChatDetailsAsync(chatId, userId);

            if (chat == null)
                return NotFound(new ApiError("ChatNotFound"));

            return Ok(new ApiResponse(chat));
        }

        // POST: Create Chat 
        // (PRIVATE or GROUP depending on DTO)
        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var creatorId = GetUserId();

            // If request.UserId != null → private chat
            if (!string.IsNullOrWhiteSpace(request.ParticipantId))
            {
                var chat = await _chatService.GetOrCreatePrivateChatAsync(creatorId, request.ParticipantId);
                return Ok(new ApiResponse(chat));
            }

            // Otherwise → group chat
            var groupChat = await _chatService.CreateChatAsync(creatorId, request);

            return CreatedAtAction(
                nameof(GetChatDetails),
                new { chatId = groupChat.Id },
                new ApiResponse(groupChat)
            );
        }

        // PUT: Update Chat
        [HttpPut("{chatId:int}")]
        public async Task<IActionResult> UpdateChat(int chatId, [FromBody] UpdateChatRequestDto request)
        {
            var result = await _chatService.UpdateChatAsync(chatId, request);

            if (result == null)
                return NotFound(new ApiError("ChatNotFound"));

            return Ok(new ApiResponse(result));
        }

        // DELETE: Delete Chat
        [HttpDelete("{chatId:int}")]
        public async Task<IActionResult> DeleteChat(int chatId)
        {
            var success = await _chatService.DeleteChatAsync(chatId);

            if (!success)
                return NotFound(new ApiError("ChatNotFound"));

            return Ok(ApiResponse.Ok(null, "Chat deleted successfully"));
        }

        // POST: Add Participant
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

        // DELETE: Remove Participant
        [HttpDelete("{chatId:int}/participants/{userId}")]
        public async Task<IActionResult> RemoveParticipant(int chatId, string userId)
        {
            var result = await _chatService.RemoveParticipantAsync(chatId, userId);

            if (!result)
                return NotFound(new ApiError("ParticipantNotFound"));

            return Ok(ApiResponse.Ok(null, "Participant removed successfully"));
        }

        // DELETE: Leave Chat
        [HttpDelete("{chatId:int}/leave")]
        public async Task<IActionResult> LeaveChat(int chatId)
        {
            var userId = GetUserId();

            var success = await _chatService.LeaveChatAsync(chatId, userId);

            if (!success)
                return NotFound(new ApiError("UserNotInChat"));

            return Ok(ApiResponse.Ok(null, "Successfully left the chat"));
        }

        // POST: Mark chat messages as read
        [HttpPost("{chatId:int}/mark-read")]
        public async Task<IActionResult> MarkChatAsRead(int chatId)
        {
            var userId = GetUserId();

            var result = await _chatService.MarkChatAsReadAsync(chatId, userId);

            if (!result)
                return NotFound(new ApiError("ChatNotFound"));

            return Ok(ApiResponse.Ok(null, "Messages marked as read"));
        }

        // GET: Unread messages count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetUserId();

            var count = await _chatService.GetUnreadMessagesCountAsync(userId);
            return Ok(ApiResponse.Ok(new { UnreadCount = count }));
        }

        // GET: Recent user chats summary
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentChats()
        {
            var userId = GetUserId();

            var chats = await _chatService.GetRecentChatsAsync(userId);
            return Ok(ApiResponse.Ok(chats));
        }
    }
}
