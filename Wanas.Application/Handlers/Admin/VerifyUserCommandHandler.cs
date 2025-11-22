using MediatR;
using Microsoft.AspNetCore.Identity;
using Wanas.Application.Commands.Admin;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;

namespace Wanas.Application.Handlers.Admin
{
    public class VerifyUserCommandHandler : IRequestHandler<VerifyUserCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _audit;

        public VerifyUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            IAuditLogService audit)
        {
            _userManager = userManager;
            _audit = audit;
        }

        public async Task<bool> Handle(VerifyUserCommand request, CancellationToken cancellationToken)
        {
            // Find the user
            var user = await _userManager.FindByIdAsync(request.TargetUserId);
            if (user == null) return false;

            // Check if user is already verified
            if (user.IsVerified)
            {
                // Already verified, no action needed
                return false;
            }

            // Check if user is banned or suspended (cannot verify banned/suspended users)
            if (user.IsBanned || user.IsSuspended)
            {
                // Cannot verify banned or suspended users
                return false;
            }

            // Verify the user
            user.IsVerified = true;

            // Update user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return false;

            // Log the action
            var details = $"Reason={request.Reason ?? "Admin verification"}";
            await _audit.LogAsync("VerifyUser", request.AdminId, request.TargetUserId, details);

            return true;
        }
    }
}
