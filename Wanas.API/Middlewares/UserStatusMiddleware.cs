using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Wanas.Domain.Entities;

namespace Wanas.API.Middlewares
{
    /// <summary>
    /// Middleware to check if user is banned or suspended before processing requests
    /// </summary>
    public class UserStatusMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserStatusMiddleware> _logger;

        public UserStatusMiddleware(RequestDelegate next, ILogger<UserStatusMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
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

                    _logger.LogInformation("Auto-lifted expired suspension for user {UserId}", userId);
                }
                else
                {
                    // Suspension still active
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
    }
}
