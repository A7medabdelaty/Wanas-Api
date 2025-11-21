using System.Net.Http.Json;
using System.Text.Json;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
namespace Wanas.Application.Services
{
    public class ChromaService : IChromaService
    {
        private readonly HttpClient _httpClient;
        private readonly IEmbeddingService _embeddingService;
        private readonly string _baseUrl = "http://localhost:8000/api/v1";

        public ChromaService(HttpClient httpClient, IEmbeddingService embeddingService)
        {
            _httpClient = httpClient;
            _embeddingService = embeddingService;
        }

        public async Task InitializeCollectionAsync()
        {
            var collectionPayload = new
            {
                name = "listings",
                metadata = new { description = "Property listings for semantic search" }
            };

            try
            {
                await _httpClient.PostAsJsonAsync($"{_baseUrl}/collections", collectionPayload);
            }
            catch (HttpRequestException ex)
            {
                // Collection might already exist, continue
                Console.WriteLine($"Collection init: {ex.Message}");
            }
        }

        public async Task AddListingEmbeddingsAsync(Listing listing)
        {
            var textToEmbed = $"{listing.Title} {listing.Description} {listing.City} {listing.User?.Bio}";
            var embeddings = await _embeddingService.GenerateEmbeddingsAsync(textToEmbed);

            var payload = new
            {
                ids = new[] { listing.Id.ToString() },
                embeddings = new[] { embeddings },
                metadatas = new[] {
                new {
                    title = listing.Title,
                    description = listing.Description,
                    city = listing.City,
                    userId = listing.UserId,
                    ownerBio = listing.User?.Bio ?? "",
                    listingId = listing.Id
                }
            },
                documents = new[] { textToEmbed }
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/collections/listings/add", payload);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<int>> SemanticSearchAsync(string query, int topK = 10)
        {
            var queryEmbeddings = await _embeddingService.GenerateEmbeddingsAsync(query);

            var payload = new
            {
                query_embeddings = new[] { queryEmbeddings },
                n_results = topK
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/collections/listings/query", payload);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(content);

            var ids = document.RootElement
                .GetProperty("ids")[0]
                .EnumerateArray()
                .Select(x => int.Parse(x.GetString()))
                .ToList();

            return ids;
        }
    }
}
