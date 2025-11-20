using MediatR;

namespace Wanas.Application.Commands.Admin
{
    public record SuspendUserCommand(
      string TargetUserId, 
      string AdminId, 
       int? DurationDays, 
            string? Reason
        ) : IRequest<bool>;
}
