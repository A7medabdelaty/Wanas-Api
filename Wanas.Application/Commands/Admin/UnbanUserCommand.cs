using MediatR;

namespace Wanas.Application.Commands.Admin
{
    public record UnbanUserCommand(
        string TargetUserId,
        string AdminId,
        string? Reason
    ) : IRequest<bool>;
}
