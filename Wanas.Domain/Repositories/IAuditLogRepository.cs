using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        // Repository-specific queries can go here if needed
        // e.g., Task<IEnumerable<AuditLog>> GetLogsByAdminIdAsync(string adminId);
    }
}
