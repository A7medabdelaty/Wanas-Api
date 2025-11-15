using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
    public interface IUserPreferenceRepository
    {
        Task<UserPreference> GetByUserIdAsync(string userId);
    }
}
