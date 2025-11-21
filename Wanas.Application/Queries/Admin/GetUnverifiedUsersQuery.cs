using MediatR;
using Wanas.Application.DTOs;

namespace Wanas.Application.Queries.Admin
{
    /// <summary>
    /// Query to get unverified users for admin review
    /// </summary>
    public record GetUnverifiedUsersQuery() : IRequest<IEnumerable<UserDto>>;
}
