using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wanas.Application.Interfaces;

namespace Wanas.API.Controllers
{
    [ApiController]
    [Route("api/admin/chroma")]
    //[Authorize(Roles = "Admin")]
    public class AdminChromaIndexingController : ControllerBase
    {
        private readonly IChromaIndexingService _indexingService;
        private readonly ILogger<AdminChromaIndexingController> _logger;

        public AdminChromaIndexingController(
              IChromaIndexingService indexingService,
              ILogger<AdminChromaIndexingController> logger)
        {
            _indexingService = indexingService;
            _logger = logger;
        }

        /// <summary>
        /// Re-index all active listings to ChromaDB
        /// </summary>
        [HttpPost("reindex-all")]
        public async Task<IActionResult> ReindexAll()
        {
            _logger.LogInformation("Admin triggered bulk reindexing of all listings");

            var result = await _indexingService.IndexAllListingsAsync();

            return Ok(new
            {
                message = "Bulk indexing completed",
                totalProcessed = result.TotalProcessed,
                successCount = result.SuccessCount,
                failedCount = result.FailedCount,
                errors = result.Errors
            });
        }

        /// <summary>
        /// Index listings created in the last N hours
        /// </summary>
        [HttpPost("index-recent")]
        public async Task<IActionResult> IndexRecent([FromQuery] int hours = 24)
        {
            _logger.LogInformation("Admin triggered indexing of listings from last {Hours} hours", hours);

            var since = DateTime.UtcNow.AddHours(-hours);
            var result = await _indexingService.IndexRecentListingsAsync(since);

            return Ok(new
            {
                message = $"Indexed listings from last {hours} hours",
                since = since,
                totalProcessed = result.TotalProcessed,
                successCount = result.SuccessCount,
                failedCount = result.FailedCount,
                errors = result.Errors
            });
        }

        /// <summary>
        /// Index a specific listing by ID
        /// </summary>
        [HttpPost("index-listing/{id}")]
        public async Task<IActionResult> IndexListing(int id)
        {
            _logger.LogInformation("Admin triggered indexing of listing {ListingId}", id);

            var success = await _indexingService.IndexListingAsync(id);

            if (!success)
                return NotFound(new { message = "Listing not found or indexing failed" });

            return Ok(new
            {
                message = "Listing indexed successfully",
                listingId = id
            });
        }

        /// <summary>
        /// Remove a listing from ChromaDB index
        /// </summary>
        [HttpDelete("remove-listing/{id}")]
        public async Task<IActionResult> RemoveListing(int id)
        {
            _logger.LogInformation("Admin triggered removal of listing {ListingId} from index", id);

            var success = await _indexingService.RemoveListingFromIndexAsync(id);

            if (!success)
                return NotFound(new { message = "Listing not found in index or removal failed" });

            return Ok(new
            {
                message = "Listing removed from index",
                listingId = id
            });
        }

        /// <summary>
        /// Check ChromaDB index status
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var hasDocuments = await _indexingService.HasAnyDocumentsAsync();

            return Ok(new
            {
                chromaDbConnected = hasDocuments || true, // If we got here, connection works
                hasIndexedListings = hasDocuments,
                message = hasDocuments
           ? "ChromaDB is operational and has indexed listings"
       : "ChromaDB is operational but has no indexed listings. Consider running bulk reindex."
            });
        }
    }
}
