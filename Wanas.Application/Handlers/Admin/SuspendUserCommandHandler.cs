using MediatR;
using Microsoft.AspNetCore.Identity;
using Wanas.Domain.Entities;
using Wanas.Application.Interfaces;
using Wanas.Application.Commands.Admin;

namespace Wanas.Application.Handlers.Admin
{
    public class SuspendUserCommandHandler
    : IRequestHandler<SuspendUserCommand, SuspendResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _audit;

        public SuspendUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            IAuditLogService audit)
        {
            _userManager = userManager;
            _audit = audit;
        }

        public async Task<SuspendResult> Handle(SuspendUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.TargetUserId);
            if (user == null)
                return new SuspendResult(false, false, null);

            // Prevent suspending admins
            var isTargetAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isTargetAdmin)
                return new SuspendResult(false, false, null);

            // --- Prevent duplicate suspension ---
            if (user.IsSuspended)
            {
                if (user.SuspendedUntil.HasValue && user.SuspendedUntil > DateTime.UtcNow)
                    return new SuspendResult(false, true, user.SuspendedUntil);

                return new SuspendResult(false, true, null);
            }

            // Apply suspension
            user.IsSuspended = true;
            if (request.DurationDays.HasValue)
                user.SuspendedUntil = DateTime.UtcNow.AddDays(request.DurationDays.Value);
            else
                user.SuspendedUntil = null;

            await _userManager.UpdateSecurityStampAsync(user);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return new SuspendResult(false, false, null);

            await _audit.LogAsync("SuspendUser", request.AdminId, request.TargetUserId,
                $"DurationDays={request.DurationDays}; Reason={request.Reason}");

            return new SuspendResult(true, false, user.SuspendedUntil);
        }
    }

}
