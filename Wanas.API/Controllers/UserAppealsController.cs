using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wanas.Application.Commands.User;
using Wanas.Application.Queries.User;
using Wanas.Domain.Enums;

namespace Wanas.API.Controllers
{
    [ApiController]
    [Route("api/user/appeals")]
    [Authorize] // Any authenticated user can submit an appeal
    public class UserAppealsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserAppealsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST api/user/appeals
        [HttpPost]
        public async Task<IActionResult> SubmitAppeal([FromBody] SubmitAppealRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                 ?? string.Empty;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var command = new SubmitAppealCommand(
                UserId: userId,
                AppealType: request.AppealType,
                Reason: request.Reason
            );

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(new
            {
                message = "Appeal submitted successfully.",
                appealId = result.Value,
                submittedAt = DateTime.UtcNow
            });
        }

        // GET api/user/appeals/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyAppeals()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? string.Empty;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = new GetMyAppealsQuery(userId);
            var appeals = await _mediator.Send(query);

            return Ok(new
            {
                totalCount = appeals.Count(),
                appeals
            });
        }
    }

    public class SubmitAppealRequest
    {
        public AppealType AppealType { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
