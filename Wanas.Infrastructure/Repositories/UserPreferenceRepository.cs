using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class UserPreferenceRepository : GenericRepository<UserPreference>, IUserPreferenceRepository
    {
        public UserPreferenceRepository(AppDBContext context) : base(context)
        {
        }

        public async Task<UserPreference> GetByUserIdAsync(string userId)
        {
            return await _context.UserPreferences
                .FirstOrDefaultAsync(up => up.UserId == userId);
        }
    }
}
