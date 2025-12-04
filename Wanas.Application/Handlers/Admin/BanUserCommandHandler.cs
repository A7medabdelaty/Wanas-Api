using Microsoft.AspNetCore.Identity;
using Wanas.Application.Commands.Admin;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using MediatR;

namespace Wanas.Application.Handlers.Admin
{
    public class BanUserCommandHandler : IRequestHandler<BanUserCommand, BanResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _audit;

        public BanUserCommandHandler(UserManager<ApplicationUser> userManager, IAuditLogService audit)
        {
            _userManager = userManager;
            _audit = audit;
        }

        public async Task<BanResult> Handle(BanUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.TargetUserId);
            if (user == null)
                return new BanResult(false, false, "User not found");

            var isTargetAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isTargetAdmin)
                return new BanResult(false, false, "Cannot ban admin user");

            // Prevent duplicate ban
            if (user.IsBanned)
                return new BanResult(false, true, "User is already banned");

            user.IsBanned = true;

            await _userManager.UpdateSecurityStampAsync(user);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return new BanResult(false, false, "Failed to update user");

            var details = $"Reason={request.Reason}";
            await _audit.LogAsync("BanUser", request.AdminId, request.TargetUserId, details);

            return new BanResult(true, false, "User banned successfully");
        }
    }
}
