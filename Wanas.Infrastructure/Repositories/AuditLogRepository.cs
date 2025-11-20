using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(AppDBContext context) : base(context)
        {
        }

        // Add repository-specific data access methods here if needed
        // e.g., public async Task<IEnumerable<AuditLog>> GetLogsByAdminIdAsync(string adminId)
        //       => await FindAsync(log => log.AdminId == adminId);
    }
}
