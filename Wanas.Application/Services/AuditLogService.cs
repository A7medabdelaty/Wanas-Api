using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _db;

        public AuditLogService(IUnitOfWork db)
        {
            _db = db;
        }

        public async Task LogAsync(string action, string adminId, string targetUserId, string? details = null)
        {
            var log = new AuditLog
            {
                Action = action,
                AdminId = adminId,
                TargetUserId = targetUserId,
                Details = details
            };

            await _db.AuditLogs.AddAsync(log);
            await _db.CommitAsync();
        }
    }
}
