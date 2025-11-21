using MediatR;
using Microsoft.AspNetCore.Identity;
using Wanas.Application.Commands.Admin;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;

namespace Wanas.Application.Handlers.Admin
{
    public class UnsuspendUserCommandHandler : IRequestHandler<UnsuspendUserCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _audit;

        public UnsuspendUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            IAuditLogService audit)
        {
            _userManager = userManager;
            _audit = audit;
        }

        public async Task<bool> Handle(UnsuspendUserCommand request, CancellationToken cancellationToken)
        {
            // Find the user
            var user = await _userManager.FindByIdAsync(request.TargetUserId);
            if (user == null) return false;

            // Check if user is actually suspended
            if (!user.IsSuspended)
            {
                // User is not suspended, nothing to do
                return false;
            }

            // Remove suspension
            user.IsSuspended = false;
            user.SuspendedUntil = null;

            // Update security stamp to ensure fresh authentication
            // This is optional but recommended for security consistency
            await _userManager.UpdateSecurityStampAsync(user);

            // Update user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return false;

            // Log the action
            var details = $"Reason={request.Reason ?? "Manual unsuspend"}";
            await _audit.LogAsync("UnsuspendUser", request.AdminId, request.TargetUserId, details);

            return true;
        }
    }
}
