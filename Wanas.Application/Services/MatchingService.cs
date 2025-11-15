using Wanas.Application.DTOs.Matching;
using Wanas.Application.Interfaces;
using Wanas.Domain.Repositories;
using Wanas.Domain.Entities;

namespace Wanas.Application.Services
{
    public class MatchingService : IMatchingService
    {
        private readonly IUserRepository _userRepo;
        private readonly IUserPreferenceRepository _prefRepo;
        private readonly IListingRepository _listingRepo;

        // Scoring constants
        private const int CityMatchScore = 30;
        private const int AgeCompatibilityScore = 20;
        private const int BudgetCompatibilityScore = 20;
        private const int GenderPreferenceScore = 5;
        private const int SmokingCompatibilityScore = 10;
        private const int PetsCompatibilityScore = 5;
        private const int SleepScheduleCompatibilityScore = 5;
        private const int SocialLevelCompatibilityScore = 5;
        private const int MinimumScoreThreshold = 20;

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
            var (user, preferences) = await ValidateAndGetUserDataAsync(userId);
            if (user == null || preferences == null)
                return new List<MatchingResultDto>();

            var listings = await _listingRepo.GetActiveListingsAsync();
            var results = CalculateMatches(userId, preferences, listings);

            return results.OrderByDescending(r => r.Score).ToList();
        }

        private async Task<(ApplicationUser, UserPreference)> ValidateAndGetUserDataAsync(string userId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null || user.IsDeleted)
                return (null, null);

            var preferences = await _prefRepo.GetByUserIdAsync(userId);
            if (preferences == null)
                return (null, null);

            return (user, preferences);
        }

        private List<MatchingResultDto> CalculateMatches(string userId, UserPreference userPref, List<Listing> listings)
        {
            var results = new List<MatchingResultDto>();

            foreach (var listing in listings)
            {
                if (!IsValidListing(listing, userId))
                    continue;

                int score = CalculateCompatibilityScore(userPref, listing);

                if (score >= MinimumScoreThreshold)
                {
                    results.Add(CreateMatchingResult(listing, score));
                }
            }

            return results;
        }

        private bool IsValidListing(Listing listing, string userId)
        {
            return listing.UserId != userId && 
                   listing.User != null && 
                   listing.User.UserPreference != null;
        }

        private int CalculateCompatibilityScore(UserPreference userPref, Listing listing)
        {
            int score = 0;

            score += CalculateCityScore(userPref.City, listing.City);
            score += CalculateAgeScore(userPref, listing.User.Age);
            score += CalculateBudgetScore(userPref, listing.ApartmentListing);
            score += CalculateGenderScore(userPref, listing.User.UserPreference);
            score += CalculateSmokingScore(userPref, listing.User.UserPreference);
            score += CalculatePetsScore(userPref, listing.User.UserPreference);
            score += CalculateSleepScheduleScore(userPref, listing.User.UserPreference);
            score += CalculateSocialLevelScore(userPref, listing.User.UserPreference);

            return score;
        }

        private int CalculateCityScore(string userCity, string listingCity)
        {
            if (!string.IsNullOrEmpty(listingCity) &&
                !string.IsNullOrEmpty(userCity) &&
                listingCity.Equals(userCity, StringComparison.OrdinalIgnoreCase))
            {
                return CityMatchScore;
            }
            return 0;
        }

        private int CalculateAgeScore(UserPreference userPref, int ownerAge)
        {
            if (ownerAge >= userPref.MinimumAge && ownerAge <= userPref.MaximumAge)
                return AgeCompatibilityScore;
            return 0;
        }

        private int CalculateBudgetScore(UserPreference userPref, ApartmentListing apartmentListing)
        {
            if (apartmentListing == null)
                return 0;

            var monthlyPrice = apartmentListing.MonthlyPrice;
            if (monthlyPrice >= userPref.MinimumBudget && monthlyPrice <= userPref.MaximumBudget)
                return BudgetCompatibilityScore;
            return 0;
        }

        private int CalculateGenderScore(UserPreference userPref, UserPreference ownerPref)
        {
            return userPref.Gender == ownerPref.Gender ? GenderPreferenceScore : 0;
        }

        private int CalculateSmokingScore(UserPreference userPref, UserPreference ownerPref)
        {
            return userPref.Smoking == ownerPref.Smoking ? SmokingCompatibilityScore : 0;
        }

        private int CalculatePetsScore(UserPreference userPref, UserPreference ownerPref)
        {
            return userPref.Pets == ownerPref.Pets ? PetsCompatibilityScore : 0;
        }

        private int CalculateSleepScheduleScore(UserPreference userPref, UserPreference ownerPref)
        {
            return userPref.SleepSchedule == ownerPref.SleepSchedule ? SleepScheduleCompatibilityScore : 0;
        }

        private int CalculateSocialLevelScore(UserPreference userPref, UserPreference ownerPref)
        {
            return userPref.SocialLevel == ownerPref.SocialLevel ? SocialLevelCompatibilityScore : 0;
        }

        private MatchingResultDto CreateMatchingResult(Listing listing, int score)
        {
            return new MatchingResultDto
            {
                ListingId = listing.Id,
                ListingTitle = listing.Title,
                ListingCity = listing.City,
                OwnerName = listing.User.FullName,
                OwnerPhoto = listing.User.Photo,
                Score = score
            };
        }
    }
}
