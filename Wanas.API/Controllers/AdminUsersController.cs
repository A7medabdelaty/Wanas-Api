using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wanas.Application.Commands.Admin;
using Wanas.Application.Queries.Admin;
using Wanas.Domain.Enums;

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

        // POST api/admin/users/{id}/unsuspend
        [HttpPost("{id}/unsuspend")]
        public async Task<IActionResult> UnsuspendUser(string id, [FromBody] UnsuspendUserRequest request)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub")
                 ?? string.Empty;

            var command = new UnsuspendUserCommand(
                TargetUserId: id,
                AdminId: adminId,
                Reason: request.Reason
            );

            var result = await _mediator.Send(command);

            if (!result)
                return NotFound(new { message = "User not found, not suspended, or operation failed." });

            return Ok(new
            {
                message = "User suspension lifted successfully.",
                userId = id,
                unsuspendedAt = DateTime.UtcNow
            });
        }

        // POST api/admin/users/{id}/unban
        [HttpPost("{id}/unban")]
        public async Task<IActionResult> UnbanUser(string id, [FromBody] UnbanUserRequest request)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? string.Empty;

            var command = new UnbanUserCommand(
                TargetUserId: id,
                AdminId: adminId,
                Reason: request.Reason
            );

            var result = await _mediator.Send(command);

            if (!result)
                return NotFound(new { message = "User not found, not banned, or operation failed." });

            return Ok(new
            {
                message = "User ban lifted successfully.",
                userId = id,
                unbannedAt = DateTime.UtcNow
            });
        }

        // POST api/admin/users/{id}/verify
        [HttpPost("{id}/verify")]
        public async Task<IActionResult> VerifyUser(string id, [FromBody] VerifyUserRequest request)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? string.Empty;

            var command = new VerifyUserCommand(
                TargetUserId: id,
                AdminId: adminId,
                Reason: request.Reason
            );

            var result = await _mediator.Send(command);

            if (!result)
                return NotFound(new { message = "User not found, already verified, or cannot be verified (banned/suspended)." });

            return Ok(new
            {
                message = "User verified successfully.",
                userId = id,
                verifiedAt = DateTime.UtcNow
            });
        }

        // POST api/admin/users/{id}/unverify
        [HttpPost("{id}/unverify")]
        public async Task<IActionResult> UnverifyUser(string id, [FromBody] UnverifyUserRequest request)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? string.Empty;

            var command = new UnverifyUserCommand(
                TargetUserId: id,
                AdminId: adminId,
                Reason: request.Reason
            );

            var result = await _mediator.Send(command);

            if (!result)
                return NotFound(new { message = "User not found or already unverified." });

            return Ok(new
            {
                message = "User unverified successfully.",
                userId = id,
                unverifiedAt = DateTime.UtcNow
            });
        }

        // GET api/admin/users/unverified
        [HttpGet("unverified")]
        public async Task<IActionResult> GetUnverifiedUsers()
        {
            var query = new GetUnverifiedUsersQuery();
            var users = await _mediator.Send(query);

            return Ok(new
            {
                totalCount = users.Count(),
                users
            });
        }

        // GET api/admin/appeals
        [HttpGet("appeals")]
        public async Task<IActionResult> GetAppeals([FromQuery] AppealStatus? status = null)
        {
            var query = new GetAppealsQuery(status);
            var appeals = await _mediator.Send(query);

            return Ok(new
            {
                totalCount = appeals.Count(),
                appeals
            });
        }

        // POST api/admin/appeals/{id}/review
        [HttpPost("appeals/{id}/review")]
        public async Task<IActionResult> ReviewAppeal(Guid id, [FromBody] ReviewAppealRequest request)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
               ?? string.Empty;

            var command = new ReviewAppealCommand(
                AppealId: id,
                AdminId: adminId,
                IsApproved: request.IsApproved,
                AdminResponse: request.AdminResponse
            );

            var result = await _mediator.Send(command);

            if (!result)
                return NotFound(new { message = "Appeal not found or already reviewed." });

            return Ok(new
            {
                message = request.IsApproved ? "Appeal approved successfully." : "Appeal rejected.",
                appealId = id,
                reviewedAt = DateTime.UtcNow
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

    public class UnsuspendUserRequest
    {
        public string? Reason { get; set; }
    }

    public class UnbanUserRequest
    {
        public string? Reason { get; set; }
    }

    public class VerifyUserRequest
    {
        public string? Reason { get; set; }
    }

    public class UnverifyUserRequest
    {
        public string? Reason { get; set; }
    }

    public class ReviewAppealRequest
    {
        public bool IsApproved { get; set; }
        public string? AdminResponse { get; set; }
    }
}
