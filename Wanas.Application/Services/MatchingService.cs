using Wanas.Application.DTOs.Matching;
using Wanas.Application.Interfaces;
using Wanas.Domain.Repositories;
using Wanas.Domain.Entities;

namespace Wanas.Application.Services
{
    public class MatchingService : IMatchingService
    {
        private readonly IUnitOfWork _unitOfWork;

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

        public MatchingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MatchingResultDto>> MatchUserAsync(string userId)
        {
            var (user, preferences) = await ValidateAndGetUserDataAsync(userId);
            if (user == null || preferences == null)
                return new List<MatchingResultDto>();

            var listings = await _unitOfWork.Listings.GetActiveListingsAsync();

            var results = CalculateMatches(userId, preferences, listings.ToList());

            return results
                .OrderByDescending(r => r.Score)
                .ToList();
        }

        private async Task<(ApplicationUser, UserPreference)> ValidateAndGetUserDataAsync(string userId)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);

            if (user == null || user.IsDeleted)
                return (null, null);

            var preferences = await _unitOfWork.UserPreferences.GetByUserIdAsync(userId);

            if (preferences == null)
                return (null, null);

            return (user, preferences);
        }

        private List<MatchingResultDto> CalculateMatches(
            string userId,
            UserPreference userPref,
            List<Listing> listings)
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
            if (listing == null)
                return false;

            if (listing.UserId == userId)
                return false;

            if (listing.User == null)
                return false;

            if (listing.User.UserPreference == null)
                return false;

            return true;
        }

        private int CalculateCompatibilityScore(UserPreference userPref, Listing listing)
        {
            int score = 0;

            // Safe null-access
            var ownerPref = listing?.User?.UserPreference;
            var ownerAge = listing?.User?.Age ?? 0;

            score += CalculateCityScore(userPref.City, listing.City);
            score += CalculateAgeScore(userPref, ownerAge);
            score += CalculateBudgetScore(userPref, listing.ApartmentListing);
            score += CalculateGenderScore(userPref, ownerPref);
            score += CalculateSmokingScore(userPref, ownerPref);
            score += CalculatePetsScore(userPref, ownerPref);
            score += CalculateSleepScheduleScore(userPref, ownerPref);
            score += CalculateSocialLevelScore(userPref, ownerPref);

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
            if (ownerAge == 0) return 0;

            if (ownerAge >= userPref.MinimumAge && ownerAge <= userPref.MaximumAge)
                return AgeCompatibilityScore;

            return 0;
        }

        private int CalculateBudgetScore(UserPreference userPref, ApartmentListing apt)
        {
            if (apt == null) return 0;

            if (apt.MonthlyPrice >= userPref.MinimumBudget &&
                apt.MonthlyPrice <= userPref.MaximumBudget)
            {
                return BudgetCompatibilityScore;
            }

            return 0;
        }

        private int CalculateGenderScore(UserPreference userPref, UserPreference ownerPref)
        {
            if (ownerPref == null) return 0;

            return userPref.Gender == ownerPref.Gender
                ? GenderPreferenceScore
                : 0;
        }

        private int CalculateSmokingScore(UserPreference userPref, UserPreference ownerPref)
        {
            if (ownerPref == null) return 0;

            return userPref.Smoking == ownerPref.Smoking
                ? SmokingCompatibilityScore
                : 0;
        }

        private int CalculatePetsScore(UserPreference userPref, UserPreference ownerPref)
        {
            if (ownerPref == null) return 0;

            return userPref.Pets == ownerPref.Pets
                ? PetsCompatibilityScore
                : 0;
        }

        private int CalculateSleepScheduleScore(UserPreference userPref, UserPreference ownerPref)
        {
            if (ownerPref == null) return 0;

            return userPref.SleepSchedule == ownerPref.SleepSchedule
                ? SleepScheduleCompatibilityScore
                : 0;
        }

        private int CalculateSocialLevelScore(UserPreference userPref, UserPreference ownerPref)
        {
            if (ownerPref == null) return 0;

            return userPref.SocialLevel == ownerPref.SocialLevel
                ? SocialLevelCompatibilityScore
                : 0;
        }

        private MatchingResultDto CreateMatchingResult(Listing listing, int score)
        {
            var firstPhotoUrl = listing.ListingPhotos?
                .OrderBy(p => p.Id)
                .FirstOrDefault()?.URL;

            var allPhotos = listing.ListingPhotos?
                .OrderBy(p => p.Id)
                .Select(p => p.URL)
                .ToList() ?? new List<string>();

            return new MatchingResultDto
            {
                ListingId = listing.Id,
                ListingTitle = listing.Title,
                ListingDescription = listing.Description,
                ListingCity = listing.City,
                Price = listing.ApartmentListing?.MonthlyPrice > 0
                    ? listing.ApartmentListing.MonthlyPrice
                    : listing.MonthlyPrice,
                FirstPhotoUrl = firstPhotoUrl,
                ListingPhotos = allPhotos,
                Score = score
            };
        }
    }
}