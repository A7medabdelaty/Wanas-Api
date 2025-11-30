using Wanas.Application.DTOs.Matching;
using Wanas.Application.Interfaces;

namespace Wanas.Application.Services
{
    public class StaticTestMatchingService : IMatchingService
    {
        private readonly IChromaService _chromaService;

        public StaticTestMatchingService(IChromaService chromaService)
        {
            _chromaService = chromaService;
        }

        public async Task<List<MatchingResultDto>> MatchUserAsync(string userId)
        {
            Console.WriteLine($"?? Starting Matching Test for user: {userId}...");

            // Create semantic query based on test user preferences
            var semanticQuery = "Apartment in Cairo. extrovert social lifestyle. late sleep schedule. " +
                               "medium noise tolerance. Smoking: allow. Pets: notallow. " +
                               "About me: I'm a software engineer who loves coding and needs quiet space for focus";

            Console.WriteLine($"?? Semantic Query: {semanticQuery}");

            // Use mock semantic search (bypass ChromaDB for now)
            var semanticListingIds = await MockSemanticSearchAsync(semanticQuery, topK: 5);

            Console.WriteLine($"?? Semantic Search Found: {string.Join(", ", semanticListingIds)}");

            // Generate test results with RAG-boosted scores
            var results = GenerateTestResults(semanticListingIds);

            Console.WriteLine($"? Matching Complete. Found {results.Count} enhanced matches");

            // Log the results
            foreach (var result in results)
            {
                Console.WriteLine($"?? Listing {result.ListingId}: {result.ListingTitle} - Score: {result.Score}");
            }

            return results;
        }

        private async Task<List<int>> MockSemanticSearchAsync(string query, int topK)
        {
            await Task.Delay(100); // Simulate async operation

            // Analyze the query and return relevant listing IDs
            if (query.Contains("quiet") || query.Contains("coding") || query.Contains("software engineer") || query.Contains("focus"))
            {
                // Return listings that match "quiet space for coding"
                return new List<int> { 1, 3, 4 }; // Quiet apartment, Student residence, Tech enthusiast
            }
            else if (query.Contains("social") || query.Contains("extrovert") || query.Contains("party"))
            {
                // Return social listings
                return new List<int> { 2, 5 }; // Social hub, Party house
            }

            // Default return
            return new List<int> { 1, 2, 3, 4, 5 }.Take(topK).ToList();
        }

        private List<MatchingResultDto> GenerateTestResults(List<int> semanticListingIds)
        {
            var allTestResults = new List<MatchingResultDto>
            {
                new() {
                    ListingId = 1,
                    ListingTitle = "Quiet Apartment for Focused Professionals",
                    ListingDescription = "Perfect for software engineers who need peace and quiet",
                    ListingCity = "Cairo",
                    Price = 3000,
                    Score = 65,
                },
                new() {
                    ListingId = 2,
                    ListingTitle = "Social Hub in Downtown Cairo",
                    ListingDescription = "Vibrant area with nightlife. Great for extroverts",
                    ListingCity = "Cairo",
                    Price = 3300,
                    Score = 45,
                },
                new() {
                    ListingId = 3,
                    ListingTitle = "Peaceful Student Residence",
                    ListingDescription = "Quiet neighborhood perfect for studying and focus",
                    ListingCity = "Cairo",
                    Price = 3600,
                    Score = 70,
                },
                new() {
                    ListingId = 4,
                    ListingTitle = "Tech Enthusiast's Paradise",
                    ListingDescription = "High-speed internet, quiet environment perfect for programmers",
                    ListingCity = "Cairo",
                    Price = 3900,
                    Score = 60,
                },
                new() {
                    ListingId = 5,
                    ListingTitle = "Party House in City Center",
                    ListingDescription = "Always something happening! Great for social people",
                    ListingCity = "Cairo",
                    Price = 4200,
                    Score = 30,
                }
            };

            // Boost scores for listings that matched semantically
            foreach (var result in allTestResults)
            {
                //1,3,4
                if (semanticListingIds.Contains(result.ListingId))
                {
                    result.Score += 25; // RAG boost
                    //result.IsRAGBoosted = true;
                    Console.WriteLine($"?? RAG Boosted Listing {result.ListingId}: {result.ListingTitle}");
                }
            }

            return allTestResults.OrderByDescending(r => r.Score).ToList();
        }
    }
}
