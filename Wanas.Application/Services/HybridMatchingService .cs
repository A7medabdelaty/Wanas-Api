using Microsoft.Extensions.Logging;
using Wanas.Application.DTOs.Matching;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class HybridMatchingService : IMatchingService
    {
        private readonly IMatchingService _traditionalMatcher;
        private readonly IChromaService _chromaService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HybridMatchingService> _logger;

        public HybridMatchingService(
            IMatchingService traditionalMatcher,
            IChromaService chromaService,
            IUnitOfWork unitOfWork,
            ILogger<HybridMatchingService> logger)
        {
            _traditionalMatcher = traditionalMatcher;
            _chromaService = chromaService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<MatchingResultDto>> MatchUserAsync(string userId)
        {
            _logger.LogInformation("Starting hybrid matching for user {UserId}", userId);

            // Step 1: Run traditional algorithm
            _logger.LogInformation("Running traditional matching algorithm");
            var traditionalResults = await _traditionalMatcher.MatchUserAsync(userId);
            _logger.LogInformation("Traditional matching returned {Count} results", traditionalResults.Count);

            // Step 2: Get user data for semantic search
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            var pref = await _unitOfWork.UserPreferences.GetByUserIdAsync(userId);

            if (user == null || pref == null)
            {
                _logger.LogWarning("User or preferences not found for {UserId}, returning traditional results only", userId);
                return traditionalResults;
            }

            try
            {
                // Step 3: Build semantic query
                var semanticQuery = BuildSemanticQuery(pref, user);
                _logger.LogInformation("Built semantic query: {Query}", semanticQuery.Substring(0, Math.Min(200, semanticQuery.Length)) + "...");

                // Step 4: Semantic search via ChromaDB + OpenAI
                _logger.LogInformation("Starting semantic search via ChromaDB");
                var semanticListingIds = await _chromaService.SemanticSearchAsync(semanticQuery, topK: 20);
                _logger.LogInformation("Semantic search returned {Count} listing IDs: [{Ids}]", 
                    semanticListingIds.Count, 
                    string.Join(", ", semanticListingIds));

                // Step 5: Enhance results with AI boost
                var enhancedResults = BoostSemanticMatches(traditionalResults, semanticListingIds);
                _logger.LogInformation("Applied semantic boost. Final result count: {Count}", enhancedResults.Count);

                return enhancedResults;
            }
            catch (Exception ex)
            {
                // Fallback to traditional if RAG/AI fails
                _logger.LogError(ex, "AI/Semantic matching failed for user {UserId}, falling back to traditional matching", userId);
                return traditionalResults;
            }
        }

        private string BuildSemanticQuery(UserPreference pref, ApplicationUser user)
        {
            var queryParts = new List<string>();

            // City is critical for roommate matching
            queryParts.Add($"Looking for a roommate in {pref.City}");

            // Lifestyle preferences - critical for compatibility
            queryParts.Add($"I am a {pref.SocialLevel.ToString().ToLower()} person who prefers {pref.SleepSchedule.ToString().ToLower()} sleep schedule");
            queryParts.Add($"I have {pref.NoiseToleranceLevel.ToString().ToLower()} noise tolerance");

            // House rules preferences - deal breakers
            var smokingPref = pref.Smoking.ToString().ToLower();
            var petsPref = pref.Pets.ToString().ToLower();
            queryParts.Add($"Smoking preference: {smokingPref}");
            queryParts.Add($"Pets preference: {petsPref}");
            
            if (pref.Children != null)
                queryParts.Add($"Children: {pref.Children.ToString().ToLower()}");
            
            if (pref.Visits != null)
                queryParts.Add($"Visits preference: {pref.Visits.ToString().ToLower()}");
            
            if (pref.OvernightGuests != null)
                queryParts.Add($"Overnight guests: {pref.OvernightGuests.ToString().ToLower()}");

            // Demographics
            queryParts.Add($"Looking for roommate aged between {pref.MinimumAge} and {pref.MaximumAge} years");
            queryParts.Add($"Budget range: {pref.MinimumBudget} EGP to {pref.MaximumBudget} EGP per month");

            // Occupation and education - helps with lifestyle matching
            if (!string.IsNullOrEmpty(pref.Job))
                queryParts.Add($"I work as a {pref.Job}");

            if (pref.IsStudent == true && !string.IsNullOrEmpty(pref.University))
            {
                var major = !string.IsNullOrEmpty(pref.Major) ? $" studying {pref.Major}" : "";
                queryParts.Add($"I am a student at {pref.University}{major}");
            }

            // Personal bio - captures personality and interests
            if (!string.IsNullOrEmpty(user.Bio))
                queryParts.Add($"About me: {user.Bio}");

            var fullQuery = string.Join(". ", queryParts);
            return fullQuery;
        }

        private List<MatchingResultDto> BoostSemanticMatches(
            List<MatchingResultDto> traditionalResults,
            List<int> semanticListingIds)
        {
            _logger.LogInformation("Applying semantic boost to {Count} traditional results", traditionalResults.Count);
            
            var results = traditionalResults.ToList();
            int boostedCount = 0;

            foreach (var result in results)
            {
                if (semanticListingIds.Contains(result.ListingId))
                {
                    var oldScore = result.Score;
                    result.Score += 15; // AI relevance boost
                    
                    // Cap score at 100 to prevent overflow
                    if (result.Score > 100)
                      result.Score = 100;
                     
                    boostedCount++;
                    _logger.LogDebug("Boosted listing {ListingId} score from {OldScore} to {NewScore}", 
                        result.ListingId, oldScore, result.Score);
                }
            }

            _logger.LogInformation("Boosted {BoostedCount} out of {TotalCount} listings with semantic match", 
                boostedCount, results.Count);

            return results.OrderByDescending(r => r.Score).ToList();
        }
    }
}
