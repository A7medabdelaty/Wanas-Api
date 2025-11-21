using MediatR;

namespace Wanas.Application.Commands.Admin
{
    public record UnverifyUserCommand(
        string TargetUserId,
        string AdminId,
        string? Reason
    ) : IRequest<bool>;
}
