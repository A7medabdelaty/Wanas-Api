using MediatR;

namespace Wanas.Application.Commands.Admin
{
    public record ReviewAppealCommand(
        Guid AppealId,
        string AdminId,
        bool IsApproved,
        string? AdminResponse
    ) : IRequest<bool>;
}
