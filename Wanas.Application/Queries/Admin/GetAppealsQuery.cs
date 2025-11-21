using MediatR;
using Wanas.Application.DTOs;
using Wanas.Domain.Enums;

namespace Wanas.Application.Queries.Admin
{
    /// <summary>
    /// Query to get all appeals (optionally filtered by status)
    /// </summary>
    public record GetAppealsQuery(
        AppealStatus? Status = null // null = all appeals, or filter by Pending/Approved/Rejected
    ) : IRequest<IEnumerable<AppealDto>>;
}
