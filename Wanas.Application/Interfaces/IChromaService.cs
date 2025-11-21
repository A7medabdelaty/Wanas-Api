using Wanas.Domain.Entities;

namespace Wanas.Application.Interfaces
{
    public interface IChromaService
    {
        Task AddListingEmbeddingsAsync(Listing listing);
        Task<List<int>> SemanticSearchAsync(string query, int topK = 10);
        Task InitializeCollectionAsync();
    }
}
