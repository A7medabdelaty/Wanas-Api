using MediatR;
using Microsoft.AspNetCore.Identity;
using Wanas.Domain.Entities;
using Wanas.Application.Interfaces;
using Wanas.Application.Commands.Admin;

namespace Wanas.Application.Handlers.Admin
{
    public class SuspendUserCommandHandler : IRequestHandler<SuspendUserCommand, bool>
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

public async Task<bool> Handle(SuspendUserCommand request, CancellationToken cancellationToken)
        {
  var user = await _userManager.FindByIdAsync(request.TargetUserId);
     if (user == null) return false;

            // Prevent suspending admins (optional safety)
   var isTargetAdmin = await _userManager.IsInRoleAsync(user, "Admin");
   if (isTargetAdmin)
         {
                // Do not suspend other admins
      return false;
   }

         user.IsSuspended = true;
  if (request.DurationDays.HasValue)
     {
       user.SuspendedUntil = DateTime.UtcNow.AddDays(request.DurationDays.Value);
   }
     else
            {
 user.SuspendedUntil = null; // null => indefinite
     }

      // Update security stamp to force token invalidation
    await _userManager.UpdateSecurityStampAsync(user);

       var updateResult = await _userManager.UpdateAsync(user);
  if (!updateResult.Succeeded) return false;

        var details = $"DurationDays={request.DurationDays}; Reason={request.Reason}";
     await _audit.LogAsync("SuspendUser", request.AdminId, request.TargetUserId, details);

   return true;
        }
    }
}
