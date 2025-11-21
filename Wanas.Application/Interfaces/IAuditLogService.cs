namespace Wanas.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(string action, string adminId, string targetUserId, string? details = null);
    }
}
