using MediatR;
using Microsoft.AspNetCore.Identity;
using Wanas.Application.Commands.Admin;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;

namespace Wanas.Application.Handlers.Admin
{
    public class UnbanUserCommandHandler : IRequestHandler<UnbanUserCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _audit;

        public UnbanUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            IAuditLogService audit)
        {
            _userManager = userManager;
            _audit = audit;
        }

        public async Task<bool> Handle(UnbanUserCommand request, CancellationToken cancellationToken)
        {
            // Find the user
            var user = await _userManager.FindByIdAsync(request.TargetUserId);
            if (user == null) return false;

            // Check if user is actually banned
            if (!user.IsBanned)
            {
                // User is not banned, nothing to do
                return false;
            }

            // Remove ban
            user.IsBanned = false;

            // Update security stamp to allow new logins
            await _userManager.UpdateSecurityStampAsync(user);

            // Update user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return false;

            // Log the action
            var details = $"Reason={request.Reason ?? "Manual unban"}";
            await _audit.LogAsync("UnbanUser", request.AdminId, request.TargetUserId, details);

            return true;
        }
    }
}
