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

        public HybridMatchingService(
            IMatchingService traditionalMatcher,
            IChromaService chromaService,
            IUnitOfWork unitOfWork)
        {
            _traditionalMatcher = traditionalMatcher;
            _chromaService = chromaService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MatchingResultDto>> MatchUserAsync(string userId)
        {
            // Step 1: Run traditional algorithm
            var traditionalResults = await _traditionalMatcher.MatchUserAsync(userId);

            // Step 2: Get user data for semantic search
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            var pref = await _unitOfWork.UserPreferences.GetByUserIdAsync(userId);

            if (user == null || pref == null)
                return traditionalResults;

            try
            {
                // Step 3: Semantic search
                var semanticQuery = BuildSemanticQuery(pref, user);
                var semanticListingIds = await _chromaService.SemanticSearchAsync(semanticQuery, topK: 20);

                // Step 4: Enhance results
                var enhancedResults = BoostSemanticMatches(traditionalResults, semanticListingIds);

                return enhancedResults;
            }
            catch (Exception ex)
            {
                // Fallback to traditional if RAG fails
                Console.WriteLine($"RAG failed, using traditional: {ex.Message}");
                return traditionalResults;
            }
        }

        private string BuildSemanticQuery(UserPreference pref, ApplicationUser user)
        {
            var queryParts = new List<string>();

            // Always include city
            queryParts.Add($"Apartment in {pref.City}");

            // Include all lifestyle preferences (all your enums have meaningful values)
            queryParts.Add($"{pref.SocialLevel.ToString().ToLower()} social lifestyle");
            queryParts.Add($"{pref.SleepSchedule.ToString().ToLower()} sleep schedule");
            queryParts.Add($"{pref.NoiseToleranceLevel.ToString().ToLower()} noise tolerance");

            // Include preference rules
            queryParts.Add($"Smoking: {pref.Smoking.ToString().ToLower()}");
            queryParts.Add($"Pets: {pref.Pets.ToString().ToLower()}");
            queryParts.Add($"Children: {pref.Children.ToString().ToLower()}");
            queryParts.Add($"Visits: {pref.Visits.ToString().ToLower()}");
            queryParts.Add($"Overnight guests: {pref.OvernightGuests.ToString().ToLower()}");

            // Include age range
            queryParts.Add($"Age range: {pref.MinimumAge} to {pref.MaximumAge}");

            // Include budget range
            queryParts.Add($"Budget: {pref.MinimumBudget} to {pref.MaximumBudget}");

            // Include education/job info if available
            if (!string.IsNullOrEmpty(pref.Job))
                queryParts.Add($"Profession: {pref.Job}");

            if (pref.IsStudent == true && !string.IsNullOrEmpty(pref.University))
                queryParts.Add($"Student at {pref.University} studying {pref.Major}");

            // Include user bio if available
            if (!string.IsNullOrEmpty(user.Bio))
                queryParts.Add($"About me: {user.Bio}");

            return string.Join(". ", queryParts);
        }
        private List<MatchingResultDto> BoostSemanticMatches(
            List<MatchingResultDto> traditionalResults,
            List<int> semanticListingIds)
        {
            var results = traditionalResults.ToList();

            foreach (var result in results)
            {
                if (semanticListingIds.Contains(result.ListingId))
                {
                    result.Score += 15; // Semantic relevance boost
                }
            }

            return results.OrderByDescending(r => r.Score).ToList();
        }
    }
}
