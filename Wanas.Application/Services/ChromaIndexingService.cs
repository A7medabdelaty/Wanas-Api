using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Wanas.Application.Interfaces;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class ChromaIndexingService : IChromaIndexingService
    {
        private readonly IChromaService _chromaService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ChromaIndexingService> _logger;
        private readonly HttpClient _httpClient;

        public ChromaIndexingService(
            IChromaService chromaService,
            IUnitOfWork unitOfWork,
            ILogger<ChromaIndexingService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _chromaService = chromaService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<bool> IndexListingAsync(int listingId)
        {
            try
            {
                _logger.LogInformation("Indexing listing {ListingId} to ChromaDB", listingId);

                var listing = await _unitOfWork.Listings.GetListingWithDetailsAsync(listingId);

                if (listing == null || !listing.IsActive)
                {
                    _logger.LogWarning("Listing {ListingId} not found or not active", listingId);
                    return false;
                }

                await _chromaService.AddListingEmbeddingsAsync(listing);
                _logger.LogInformation("Successfully indexed listing {ListingId}", listingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index listing {ListingId}", listingId);
                return false;
            }
        }

        public async Task<IndexingResult> IndexAllListingsAsync()
        {
            _logger.LogInformation("Starting bulk indexing of all active listings");
            var result = new IndexingResult();

            try
            {
                var listings = await _unitOfWork.Listings.GetActiveListingsAsync();
                result.TotalProcessed = listings.Count();

                foreach (var listing in listings)
                {
                    try
                    {
                        await _chromaService.AddListingEmbeddingsAsync(listing);
                        result.SuccessCount++;
                        _logger.LogDebug("Indexed listing {ListingId}: {Title}", listing.Id, listing.Title);
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        var error = $"Listing {listing.Id}: {ex.Message}";
                        result.Errors.Add(error);
                        _logger.LogError(ex, "Failed to index listing {ListingId}", listing.Id);
                    }
                }

                _logger.LogInformation(
                    "Bulk indexing complete. Total: {Total}, Success: {Success}, Failed: {Failed}",
                    result.TotalProcessed, result.SuccessCount, result.FailedCount
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk indexing failed");
                result.Errors.Add($"Critical error: {ex.Message}");
            }

            return result;
        }

        public async Task<IndexingResult> IndexRecentListingsAsync(DateTime since)
        {
            _logger.LogInformation("Indexing listings created since {Since}", since);
            var result = new IndexingResult();

            try
            {
                var recentListings = await _unitOfWork.Listings.FindAsync(
                    l => l.CreatedAt >= since && l.IsActive
                );

                // Load full details for each listing
                var listingsWithData = new List<Domain.Entities.Listing>();
                foreach (var listing in recentListings)
                {
                    var fullListing = await _unitOfWork.Listings.GetListingWithDetailsAsync(listing.Id);
                    if (fullListing != null)
                        listingsWithData.Add(fullListing);
                }

                result.TotalProcessed = listingsWithData.Count;

                foreach (var listing in listingsWithData)
                {
                    try
                    {
                        await _chromaService.AddListingEmbeddingsAsync(listing);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Listing {listing.Id}: {ex.Message}");
                        _logger.LogError(ex, "Failed to index listing {ListingId}", listing.Id);
                    }
                }

                _logger.LogInformation(
                    "Recent indexing complete. Total: {Total}, Success: {Success}, Failed: {Failed}",
                    result.TotalProcessed, result.SuccessCount, result.FailedCount
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Recent indexing failed");
                result.Errors.Add($"Critical error: {ex.Message}");
            }

            return result;
        }

        public async Task<bool> RemoveListingFromIndexAsync(int listingId)
        {
            try
            {
                _logger.LogInformation("Removing listing {ListingId} from ChromaDB index", listingId);

                // ChromaDB delete endpoint
                var deletePayload = new
                {
                    ids = new[] { listingId.ToString() }
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "http://localhost:8000/api/v1/collections/listings/delete",
                    deletePayload
                );

                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Successfully removed listing {ListingId} from index", listingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove listing {ListingId} from index", listingId);
                return false;
            }
        }

        public async Task<bool> HasAnyDocumentsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:8000/api/v1/collections/listings");

                if (!response.IsSuccessStatusCode)
                    return false;

                var content = await response.Content.ReadAsStringAsync();
                // Simple check - if collection exists and has data
                return content.Contains("\"count\"") && !content.Contains("\"count\":0");
            }
            catch
            {
                return false;
            }
        }
    }
}
