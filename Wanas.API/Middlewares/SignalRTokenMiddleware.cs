using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Wanas.API.Middlewares
{
    public class SignalRTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SignalRTokenMiddleware> _logger;

        public SignalRTokenMiddleware(RequestDelegate next, ILogger<SignalRTokenMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if this is a SignalR hub request
            if (context.Request.Path.StartsWithSegments("/hubs"))
            {
                var accessToken = context.Request.Query["access_token"];

                if (!string.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        // Try to add the token to the Authorization header if not already present
                        if (string.IsNullOrEmpty(context.Request.Headers["Authorization"]))
                        {
                            context.Request.Headers["Authorization"] = $"Bearer {accessToken}";
                            _logger.LogDebug("SignalR token extracted from query string and added to Authorization header");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing SignalR token from query string");
                    }
                }
            }

            await _next(context);
        }
    }
}
