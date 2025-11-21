using MediatR;

namespace Wanas.Application.Commands.Admin
{
    public record UnsuspendUserCommand(
        string TargetUserId,
        string AdminId,
        string? Reason
      ) : IRequest<bool>;
}
