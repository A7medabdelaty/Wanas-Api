using System.Collections.Generic;
using System.Threading.Tasks;
using Wanas.Application.DTOs.Matching;

namespace Wanas.Application.Interfaces
{
    public interface IRoommateMatchingService
    {
        Task<List<RoommateMatchDto>> MatchRoommatesAsync(string userId, int top = 10);
    }
}
