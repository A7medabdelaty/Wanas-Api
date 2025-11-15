using Wanas.Application.DTOs.Matching;
using Wanas.Application.Interfaces;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class MatchingService : IMatchingService
    {
        private readonly IUserRepository _userRepo;
        private readonly IUserPreferenceRepository _prefRepo;
        private readonly IListingRepository _listingRepo;

        public MatchingService(
            IUserRepository userRepo,
            IUserPreferenceRepository prefRepo,
            IListingRepository listingRepo)
        {
            _userRepo = userRepo;
            _prefRepo = prefRepo;
            _listingRepo = listingRepo;
        }

        public async Task<List<MatchingResultDto>> MatchUserAsync(string userId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null || user.IsDeleted)
                return new();

            var pref = await _prefRepo.GetByUserIdAsync(userId);
            if (pref == null)
                return new();

            var listings = await _listingRepo.GetActiveListingsAsync();

            var results = new List<MatchingResultDto>();

            foreach (var listing in listings)
            {
                // Skip own listings
                if (listing.UserId == userId)
                    continue;

                // Skip listings without user or user preference
                if (listing.User == null || listing.User.UserPreference == null)
                    continue;

                int score = 0;

                // 1. City match (30 points)
                if (!string.IsNullOrEmpty(listing.City) &&
                    !string.IsNullOrEmpty(pref.City) &&
                    listing.City.Equals(pref.City, StringComparison.OrdinalIgnoreCase))
                    score += 30;

                // 2. Owner age compatibility (20 points)
                if (listing.User.Age >= pref.MinimumAge &&
                    listing.User.Age <= pref.MaximumAge)
                    score += 20;

                // 3. Budget compatibility (20 points)
                if (listing.ApartmentListing != null)
                {
                    var monthlyPrice = listing.ApartmentListing.MonthlyPrice;
                    if (monthlyPrice >= pref.MinimumBudget &&
                        monthlyPrice <= pref.MaximumBudget)
                        score += 20;
                }

                // 4. Gender preference match (10 points)
                // User's preference should match the listing owner's actual preference
                if (pref.Gender == listing.User.UserPreference.Gender)
                    score += 5;

                // 5. Smoking compatibility (10 points)
                if (pref.Smoking == listing.User.UserPreference.Smoking)
                    score += 10;

                // 6. Pets compatibility (5 points)
                if (pref.Pets == listing.User.UserPreference.Pets)
                    score += 5;

                // 7. Sleep schedule compatibility (5 points)
                if (pref.SleepSchedule == listing.User.UserPreference.SleepSchedule)
                    score += 5;

                // 8. Social level compatibility (5 points)
                if (pref.SocialLevel == listing.User.UserPreference.SocialLevel)
                    score += 5;

                // Only include matches with reasonable compatibility (minimum 20 points)
                if (score >= 20)
                {
                    results.Add(new MatchingResultDto
                    {
                        ListingId = listing.Id,
                        ListingTitle = listing.Title,
                        ListingCity = listing.City,
                        OwnerName = listing.User.FullName,
                        OwnerPhoto = listing.User.Photo,
                        Score = score
                    });
                }
            }

            return results.OrderByDescending(r => r.Score).ToList();
        }
    }
}
