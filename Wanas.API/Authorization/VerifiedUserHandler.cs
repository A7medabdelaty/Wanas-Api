using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Wanas.Domain.Entities;

namespace Wanas.API.Authorization
{
    /// <summary>
    /// Handler to check if user is verified
    /// </summary>
    public class VerifiedUserHandler : AuthorizationHandler<VerifiedUserRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public VerifiedUserHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            VerifiedUserRequirement requirement)
        {
            // Get user ID from claims
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
            {
                return; // Not authenticated
            }

            // Get user from database
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return; // User not found
            }

            // Check if user is verified
            if (user.IsVerified)
            {
                context.Succeed(requirement);
            }
            // If not verified, requirement fails (don't call Succeed)
        }
    }
}
