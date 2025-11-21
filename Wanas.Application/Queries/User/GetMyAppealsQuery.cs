using MediatR;
using Wanas.Application.DTOs;

namespace Wanas.Application.Queries.User
{
    /// <summary>
    /// Query to get current user's appeals
    /// </summary>
    public record GetMyAppealsQuery(
      string UserId
    ) : IRequest<IEnumerable<AppealDto>>;
}
