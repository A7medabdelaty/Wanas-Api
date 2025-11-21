using MediatR;

namespace Wanas.Application.Commands.Admin
{
    public record VerifyUserCommand(
        string TargetUserId,
        string AdminId,
        string? Reason
        ) : IRequest<bool>;
}
