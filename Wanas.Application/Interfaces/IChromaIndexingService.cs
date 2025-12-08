namespace Wanas.Application.Interfaces
{
    public interface IChromaIndexingService
    {
        /// <summary>
        /// Index a single listing to ChromaDB
        /// </summary>
        Task<bool> IndexListingAsync(int listingId);

        /// <summary>
        /// Index all listings in bulk
        /// </summary>
        Task<IndexingResult> IndexAllListingsAsync();

        /// <summary>
        /// Index listings created after a specific date
        /// </summary>
        Task<IndexingResult> IndexRecentListingsAsync(DateTime since);

        /// <summary>
        /// Remove listing from ChromaDB
        /// </summary>
        Task<bool> RemoveListingFromIndexAsync(int listingId);

        /// <summary>
        /// Check if ChromaDB collection has any documents
        /// </summary>
        Task<bool> HasAnyDocumentsAsync();
    }

    public class IndexingResult
    {
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
