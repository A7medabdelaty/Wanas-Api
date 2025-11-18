using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanas.Application.DTOs.AI;

namespace Wanas.Application.Interfaces.AI
{
    public interface IGenerateListingService
    {
        Task<GeneratedListingResponseDto> GenerateListingAsync(GenerateListingRequestDto request , string ownerId);

    }
}
