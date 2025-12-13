using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Wanas.Domain.Entities;
using Wanas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Enums;

namespace Wanas.API.Middlewares
{
    /// <summary>
    /// Middleware to check if user is banned or suspended before processing requests
    /// </summary>
    public class UserStatusMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserStatusMiddleware> _logger;

        // Endpoints that should bypass the status check
        private static readonly HashSet<string> WhitelistedPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/api/auth",          // Login endpoint (POST)
            "/api/auth/refresh",  // Refresh token
            "/api/auth/register",         // Registration
            "/api/auth/confirm-email",      // Email confirmation
            "/api/auth/resend-confirmation-email",
            "/api/auth/forget-password",    // Password reset
            "/api/auth/reset-password",
            "/api/user/status",     // User status check endpoint
            "api/user/appeals",          // Submitting appeals
            "api/user/appeals/my", // Viewing own appeals
        };

        public UserStatusMiddleware(RequestDelegate next, ILogger<UserStatusMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, AppDBContext db)
        {
            // Skip check for whitelisted endpoints
            if (IsWhitelistedPath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Skip check for anonymous requests or public endpoints
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            // Get user ID from claims
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
            {
                await _next(context);
                return;
            }

            // Get user from database
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await _next(context);
                return;
            }

            // Check if user is banned
            if (user.IsBanned)
            {
                // Deactivate listings for banned users
                await DeactivateUserListingsAsync(db, userId);

                _logger.LogWarning("Banned user {UserId} attempted to access {Path}", userId, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Account Banned",
                    message = "Your account has been permanently banned. Please contact support.",
                    statusCode = 403
                });
                return;
            }

            // Check if user is suspended
            if (user.IsSuspended)
            {
                // Check if suspension has expired
                if (user.SuspendedUntil.HasValue && user.SuspendedUntil.Value <= DateTime.UtcNow)
                {
                    // Auto-lift expired suspension
                    user.IsSuspended = false;
                    user.SuspendedUntil = null;
                    await userManager.UpdateAsync(user);

                    // Reactivate only approved, non-flagged listings
                    await ReactivateEligibleListingsAsync(db, userId);

                    _logger.LogInformation("Auto-lifted expired suspension for user {UserId}", userId);
                }
                else
                {
                    // Suspension still active -> ensure listings are deactivated
                    await DeactivateUserListingsAsync(db, userId);

                    _logger.LogWarning("Suspended user {UserId} attempted to access {Path}", userId, context.Request.Path);

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    var suspensionMessage = user.SuspendedUntil.HasValue
                        ? $"Your account is suspended until {user.SuspendedUntil.Value:yyyy-MM-dd HH:mm:ss} UTC."
                        : "Your account is suspended indefinitely.";

                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Account Suspended",
                        message = suspensionMessage,
                        suspendedUntil = user.SuspendedUntil,
                        statusCode = 403
                    });
                    return;
                }
            }

            // User is not banned or suspended, continue processing
            await _next(context);
        }

        private static async Task DeactivateUserListingsAsync(AppDBContext db, string userId)
        {
            // Only deactivate those currently active
            await db.Listings
                .Where(l => l.UserId == userId && l.IsActive)
                .ExecuteUpdateAsync(setters => setters.SetProperty(l => l.IsActive, false));
        }

        private static async Task ReactivateEligibleListingsAsync(AppDBContext db, string userId)
        {
            // Reactivate only approved, non-flagged listings
            await db.Listings
                .Where(l => l.UserId == userId
                    && !l.IsActive
                    && !l.IsFlagged
                    && l.ModerationStatus == ListingModerationStatus.Approved)
                .ExecuteUpdateAsync(setters => setters.SetProperty(l => l.IsActive, true));
        }

        private static bool IsWhitelistedPath(PathString path)
        {
            return WhitelistedPaths.Any(whitelisted =>
            path.StartsWithSegments(whitelisted, StringComparison.OrdinalIgnoreCase));
        }
    }
}
