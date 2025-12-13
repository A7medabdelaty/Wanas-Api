using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wanas.Application.DTOs.Matching;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class RoommateMatchingService : IRoommateMatchingService
    {
        private readonly IUnitOfWork _unitOfWork;

        private const int CityMatchScore = 30;
        private const int AgeCompatibilityScore = 20;
        private const int BudgetCompatibilityScore = 20;
        private const int GenderPreferenceScore = 5;
        private const int SmokingCompatibilityScore = 10;
        private const int PetsCompatibilityScore = 5;
        private const int SleepScheduleCompatibilityScore = 5;
        private const int SocialLevelCompatibilityScore = 5;
        private const int MinimumScoreThreshold = 20;
        private const int MaxScore = CityMatchScore + AgeCompatibilityScore + BudgetCompatibilityScore + GenderPreferenceScore + SmokingCompatibilityScore + PetsCompatibilityScore + SleepScheduleCompatibilityScore + SocialLevelCompatibilityScore;

        public RoommateMatchingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<RoommateMatchDto>> MatchRoommatesAsync(string userId, int top = 10)
        {
            var (user, pref) = await GetUserWithPref(userId);
            if (user is null || pref is null)
                return new List<RoommateMatchDto>();

            var allUsers = await _unitOfWork.Users.GetAllUsersAsync();
            var candidates = allUsers.Where(u =>
            u.Id != userId &&
            u.UserPreference != null &&
            !(u.IsDeleted || u.IsBanned) &&
            !u.IsSuspended &&
            u.Gender == user.Gender && // Filter by ApplicationUser.Gender (same gender)
            u.ProfileType == ProfileType.Renter
            ).ToList();

            var results = new List<RoommateMatchDto>();
            foreach (var other in candidates)
            {
                var otherPref = other.UserPreference!;
                int score = 0;
                var breakdown = new MatchBreakdown();

                if (!string.IsNullOrEmpty(pref.City) && !string.IsNullOrEmpty(other.City) && pref.City.Equals(other.City, StringComparison.OrdinalIgnoreCase))
                { score += CityMatchScore; breakdown.CityMatch = true; }

                if (other.Age.HasValue && other.Age.Value >= pref.MinimumAge && other.Age.Value <= pref.MaximumAge)
                { score += AgeCompatibilityScore; breakdown.AgeCompatible = true; }

                if (RangesOverlap(pref.MinimumBudget, pref.MaximumBudget, otherPref.MinimumBudget, otherPref.MaximumBudget))
                { score += BudgetCompatibilityScore; breakdown.BudgetCompatible = true; }

                // Gender match score - now comparing ApplicationUser.Gender
                if (user.Gender == other.Gender)
                { score += GenderPreferenceScore; breakdown.GenderMatch = true; }

                if (pref.Smoking == otherPref.Smoking)
                { score += SmokingCompatibilityScore; breakdown.SmokingCompatible = true; }

                if (pref.Pets == otherPref.Pets)
                { score += PetsCompatibilityScore; breakdown.PetsCompatible = true; }

                if (pref.SleepSchedule == otherPref.SleepSchedule)
                { score += SleepScheduleCompatibilityScore; breakdown.SleepScheduleMatch = true; }

                if (pref.SocialLevel == otherPref.SocialLevel)
                { score += SocialLevelCompatibilityScore; breakdown.SocialLevelMatch = true; }

                breakdown.NoiseToleranceMatch = pref.NoiseToleranceLevel == otherPref.NoiseToleranceLevel;

                if (score >= MinimumScoreThreshold)
                {
                    var percentage = (int)Math.Round((double)score * 100 / MaxScore);
                    results.Add(new RoommateMatchDto
                    {
                        TargetUserId = other.Id,
                        FullName = other.FullName,
                        City = other.City,
                        Age = other.Age,
                        Photo = other.Photo,
                        Score = score,
                        Percentage = percentage,
                        Gender = other.Gender == Gender.Male ? "Male" : "Female", // Use ApplicationUser.Gender directly
                        Breakdown = breakdown
                    });
                }
            }

            return results
            .OrderByDescending(r => r.Score)
            .Take(top)
            .ToList();
        }

        private static bool RangesOverlap(int aMin, int aMax, int bMin, int bMax)
        => aMin <= bMax && bMin <= aMax;

        private async Task<(ApplicationUser?, UserPreference?)> GetUserWithPref(string userId)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            var pref = await _unitOfWork.UserPreferences.GetByUserIdAsync(userId);
            if (user is null || pref is null) return (null, null);
            return (user, pref);
        }
    }
}
