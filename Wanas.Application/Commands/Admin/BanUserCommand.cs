using MediatR;

namespace Wanas.Application.Commands.Admin
{
   public record BanUserCommand(
       string TargetUserId,
       string AdminId, 
      string? Reason
      ) : IRequest<bool>;
}
