using MediatR;
using Microsoft.AspNetCore.Identity;
using Wanas.Application.Commands.Admin;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;

namespace Wanas.Application.Handlers.Admin
{
    public class UnverifyUserCommandHandler : IRequestHandler<UnverifyUserCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _audit;

        public UnverifyUserCommandHandler(
         UserManager<ApplicationUser> userManager,
         IAuditLogService audit)
        {
            _userManager = userManager;
            _audit = audit;
        }

        public async Task<bool> Handle(UnverifyUserCommand request, CancellationToken cancellationToken)
        {
            // Find the user
            var user = await _userManager.FindByIdAsync(request.TargetUserId);
            if (user == null) return false;

            // Check if user is already unverified
            if (!user.IsVerified)
            {
                // Already unverified, no action needed
                return false;
            }

            // Unverify the user
            user.IsVerified = false;

            // Update user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return false;

            // Log the action
            var details = $"Reason={request.Reason ?? "Admin unverification"}";
            await _audit.LogAsync("UnverifyUser", request.AdminId, request.TargetUserId, details);

            return true;
        }
    }
}
