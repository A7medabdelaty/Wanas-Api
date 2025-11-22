using System.Diagnostics;
using System.Security.Claims;
using Serilog;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;

namespace Wanas.API.Middlewares
{
    public class TrafficLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public TrafficLoggingMiddleware(RequestDelegate next) { _next = next; }
        public async Task InvokeAsync(HttpContext context, IUnitOfWork uow)
        {
            var sw = Stopwatch.StartNew();
            await _next(context);
            sw.Stop();
            // Skip noisy paths
            var path = context.Request.Path.ToString();
            if (path.StartsWith("/swagger") || path.StartsWith("/chatHub")) return;
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Log.Information("HTTP {Method} {Path} {Status} in {Duration} ms User={UserId}", context.Request.Method, path, context.Response.StatusCode, sw.ElapsedMilliseconds, userId ?? "anon");
            // Optional: keep lightweight DB logging only for errors/slow requests
            if (context.Response.StatusCode >=500 || sw.ElapsedMilliseconds >2000)
            {
                try
                {
                    var log = new TrafficLog
                    {
                        Path = path,
                        Method = context.Request.Method,
                        UserId = string.IsNullOrEmpty(userId) ? null : userId,
                        StatusCode = context.Response.StatusCode,
                        DurationMs = sw.ElapsedMilliseconds,
                        CreatedAt = DateTime.UtcNow
                    };
                    await uow.TrafficLogs.AddAsync(log);
                    await uow.CommitAsync();
                }
                catch { }
            }
        }
    }
}