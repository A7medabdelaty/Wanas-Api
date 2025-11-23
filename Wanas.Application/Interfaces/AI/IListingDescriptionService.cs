using Wanas.Application.DTOs.AI;

namespace Wanas.Application.Interfaces.AI
{
    public interface IListingDescriptionService
    {
        Task<string> GenerateDescriptionAsync(GenerateDescriptionDto dto);

    }
}
