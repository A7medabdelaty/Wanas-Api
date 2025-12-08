using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;

namespace Wanas.Application.Services
{
    public class ChromaService : IChromaService
    {
        private readonly HttpClient _httpClient;
        private readonly IEmbeddingService _embeddingService;
        private readonly ILogger<ChromaService> _logger;
        private readonly string _baseUrl;
        private string _collectionId;
        private bool _isInitialized = false;
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

        public ChromaService(HttpClient httpClient, IEmbeddingService embeddingService, ILogger<ChromaService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _embeddingService = embeddingService;
            _logger = logger;
            
            // Try environment variable first (from .env in dev), then appsettings.json, then fallback
          _baseUrl = Environment.GetEnvironmentVariable("ChromaDB__BaseUrl")
                ?? configuration.GetValue<string>("ChromaDB:BaseUrl")
                ?? "http://localhost:8000/api/v1";
            
            _logger.LogInformation("ChromaDB initialized with base URL: {BaseUrl}", _baseUrl);
        }

        private async Task EnsureInitializedAsync()
        {
            if (_isInitialized && !string.IsNullOrEmpty(_collectionId))
                return;

            await _initLock.WaitAsync();
            try
            {
                if (_isInitialized && !string.IsNullOrEmpty(_collectionId))
                    return;

                await InitializeCollectionInternalAsync();
                _isInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        private async Task InitializeCollectionInternalAsync()
        {
            try
            {
                _logger.LogInformation("Initializing ChromaDB collection");

                // Check if collection already exists
                var getResponse = await _httpClient.GetAsync($"{_baseUrl}/collections?name=listings");

                if (getResponse.IsSuccessStatusCode)
                {
                    var content = await getResponse.Content.ReadAsStringAsync();

                    using var document = JsonDocument.Parse(content);
                    var root = document.RootElement;

                    // Response is an ARRAY
                    if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                    {
                        _collectionId = root[0].GetProperty("id").GetString();
                        _logger.LogInformation("Found existing collection with ID: {CollectionId}", _collectionId);
                        return;
                    }
                }

                // Create new collection
                var createPayload = new
                {
                    name = "listings",
                    metadata = new Dictionary<string, object>
                    {
                        ["hnsw:space"] = "cosine"
                    }
                };

                var createResponse = await _httpClient.PostAsJsonAsync($"{_baseUrl}/collections", createPayload);

                if (createResponse.IsSuccessStatusCode)
                {
                    var content = await createResponse.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(content);
                    _collectionId = document.RootElement.GetProperty("id").GetString();
                    _logger.LogInformation("Created new collection with ID: {CollectionId}", _collectionId);
                }
                else if (createResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    // Collection already exists, get it
                    var getResponse2 = await _httpClient.GetAsync($"{_baseUrl}/collections?name=listings");
                    var content = await getResponse2.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(content);

                    var root = document.RootElement;
                    if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                    {
                        _collectionId = root[0].GetProperty("id").GetString();
                        _logger.LogInformation("Retrieved existing collection with ID: {CollectionId}", _collectionId);
                    }
                }
                else
                {
                    var error = await createResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to create collection: {StatusCode}: {Error}",
                        createResponse.StatusCode, error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize ChromaDB collection");
                throw;
            }
        }

        public async Task InitializeCollectionAsync()
        {
            try
            {
                await EnsureInitializedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ChromaDB collection initialization had issues, but continuing");
            }
        }

        public async Task AddListingEmbeddingsAsync(Listing listing)
        {
            try
            {
                await EnsureInitializedAsync();

                if (string.IsNullOrEmpty(_collectionId))
                {
                    throw new InvalidOperationException("Collection ID is not initialized");
                }

                _logger.LogInformation("Generating embeddings for listing {ListingId}", listing.Id);
                var textToEmbed = $"{listing.Title} {listing.Description} {listing.City} {listing.User?.Bio}";
                var embeddings = await _embeddingService.GenerateEmbeddingsAsync(textToEmbed);

                _logger.LogInformation("Adding embeddings to ChromaDB for listing {ListingId} to collection {CollectionId}",
                    listing.Id, _collectionId);

                var payload = new
                {
                    ids = new[] { listing.Id.ToString() },
                    embeddings = new[] { embeddings },
                    metadatas = new[] {
                        new Dictionary<string, object>
                        {
                            ["title"] = listing.Title ?? "",
                            ["description"] = listing.Description ?? "",
                            ["city"] = listing.City ?? "",
                            ["userId"] = listing.UserId ?? "",
                            ["ownerBio"] = listing.User?.Bio ?? "",
                            ["listingId"] = listing.Id
                        }
                    },
                    documents = new[] { textToEmbed }
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/collections/{_collectionId}/add",
                    payload
                );

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully added embeddings for listing {ListingId}", listing.Id);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("ChromaDB add failed with {StatusCode}: {Error}", response.StatusCode, errorContent);

                    // If collection not found, reset and retry
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("Collection not found, resetting initialization");
                        _isInitialized = false;
                        _collectionId = null;
                        await AddListingEmbeddingsAsync(listing);
                        return;
                    }

                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add embeddings for listing {ListingId}", listing.Id);
                throw;
            }
        }

        public async Task<List<int>> SemanticSearchAsync(string query, int topK = 10)
        {
            try
            {
                await EnsureInitializedAsync();

                if (string.IsNullOrEmpty(_collectionId))
                {
                    throw new InvalidOperationException("Collection ID is not initialized");
                }

                _logger.LogInformation("Starting semantic search with query: {Query}", query);
                var queryEmbeddings = await _embeddingService.GenerateEmbeddingsAsync(query);

                var payload = new
                {
                    query_embeddings = new[] { queryEmbeddings },
                    n_results = topK
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/collections/{_collectionId}/query",
                    payload
                );

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(content);

                var ids = document.RootElement
                    .GetProperty("ids")[0]
                    .EnumerateArray()
                    .Select(x => int.Parse(x.GetString()))
                    .ToList();

                _logger.LogInformation("ChromaDB returned {Count} semantic matches", ids.Count);
                return ids;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Semantic search failed");
                throw;
            }
        }
    }
}