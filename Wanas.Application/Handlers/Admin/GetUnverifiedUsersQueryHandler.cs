using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wanas.Application.DTOs;
using Wanas.Application.Queries.Admin;
using Wanas.Domain.Entities;

namespace Wanas.Application.Handlers.Admin
{
    public class GetUnverifiedUsersQueryHandler : IRequestHandler<GetUnverifiedUsersQuery, IEnumerable<UserDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetUnverifiedUsersQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<UserDto>> Handle(GetUnverifiedUsersQuery request, CancellationToken cancellationToken)
        {
            // Get all users who are not verified and not banned/suspended
            var unverifiedUsers = await _userManager.Users
                .Where(u => !u.IsVerified && !u.IsBanned && !u.IsSuspended && !u.IsDeleted)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var userDtos = unverifiedUsers.Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber,
                City = u.City,
                Age = u.Age ?? 0,
                IsVerified = u.IsVerified,
                IsSuspended = u.IsSuspended,
                IsBanned = u.IsBanned,
                CreatedAt = u.CreatedAt
            }).ToList();

            return userDtos;
        }
    }
}
