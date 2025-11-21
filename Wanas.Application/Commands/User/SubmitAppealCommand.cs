using MediatR;
using Wanas.Application.Common;
using Wanas.Domain.Enums;

namespace Wanas.Application.Commands.User
{
    public record SubmitAppealCommand(
        string UserId,
        AppealType AppealType,
        string Reason
    ) : IRequest<Result<Guid>>; // Returns Result with Appeal ID
}
