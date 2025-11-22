using MediatR;
using Microsoft.AspNetCore.Identity;
using Wanas.Application.Commands.Admin;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;

namespace Wanas.Application.Handlers.Admin
{
    public class BanUserCommandHandler : IRequestHandler<BanUserCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _audit;
        public BanUserCommandHandler(UserManager<ApplicationUser> userManager, IAuditLogService audit)
        {
            _userManager = userManager;
            _audit = audit;
        }
        public async Task<bool> Handle(BanUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.TargetUserId);
            if (user == null) return false;

            // Prevent ban admins
            var isTargetAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            // Do not ban other admins
            if (isTargetAdmin) return false;

            user.IsBanned = true;

            // Update security stamp to force token invalidation
            await _userManager.UpdateSecurityStampAsync(user);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return false;

            var details = $"Reason={request.Reason}";
            await _audit.LogAsync("BanUser", request.AdminId, request.TargetUserId, details);

            return true;

        }
    }
}
