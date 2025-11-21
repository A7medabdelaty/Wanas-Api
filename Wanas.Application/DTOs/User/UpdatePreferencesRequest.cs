using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.User;
public record UpdatePreferencesRequest(
    string? City = null,
    int? MinimumAge = null,
    int? MaximumAge = null,
    Gender? Gender = null,
    int? MinimumBudget = null,
    int? MaximumBudget = null,
    AllowOrNot? Children = null,
    AllowOrNot? Visits = null,
    AllowOrNot? OvernightGuests = null,
    AllowOrNot? Smoking = null,
    AllowOrNot? Pets = null,
    SleepSchedule? SleepSchedule = null,
    SocialLevel? SocialLevel = null,
    NoiseToleranceLevel? NoiseToleranceLevel = null,
    string? Job = null,
    bool? IsStudent = null,
    string? University = null,
    string? Major = null
);