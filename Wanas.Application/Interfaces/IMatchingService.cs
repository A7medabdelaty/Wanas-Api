using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanas.Application.DTOs.Matching;

namespace Wanas.Application.Interfaces
{
    public interface IMatchingService
    {
        Task<List<MatchingResultDto>> MatchUserAsync(string userId);
    }
}
