using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wanas.Application.Commands.Admin;

namespace Wanas.API.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminUsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST api/admin/users/{id}/suspend
        [HttpPost("{id}/suspend")]
        public async Task<IActionResult> SuspendUser(string id, [FromBody] SuspendUserRequest request)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? string.Empty;

            var command = new SuspendUserCommand(
                    TargetUserId: id,
                    AdminId: adminId,
                    DurationDays: request.DurationDays,
                    Reason: request.Reason
            );

            var result = await _mediator.Send(command);

            if (!result)
                return NotFound(new { message = "User not found or operation failed." });

            return Ok(new
            {
                message = "User suspended successfully.",
                userId = id,
                suspendedUntil = request.DurationDays.HasValue
                ? DateTime.UtcNow.AddDays(request.DurationDays.Value)
                : (DateTime?)null
            });
        }

        // POST api/admin/users/{id}/ban
        [HttpPost("{id}/ban")]
        public async Task<IActionResult> BanUser(string id, [FromBody] BanUserRequest request)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? string.Empty;

            var command = new BanUserCommand(
                TargetUserId: id,
                AdminId: adminId,
                Reason: request.Reason
            );

            var result = await _mediator.Send(command);

            if (!result)
                return NotFound(new { message = "User not found or operation failed." });

            return Ok(new
            {
                message = "User banned permanently.",
                userId = id,
                bannedAt = DateTime.UtcNow
            });
        }
    }

    public class SuspendUserRequest
    {
        public int? DurationDays { get; set; }
        public string? Reason { get; set; }
    }

    public class BanUserRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
