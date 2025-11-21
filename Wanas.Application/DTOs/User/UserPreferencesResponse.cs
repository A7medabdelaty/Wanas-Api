using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.User;
public record UserPreferencesResponse(
    int Id,
    string City,
    int MinimumAge,
    int MaximumAge,
    Gender Gender,
    int MinimumBudget,
    int MaximumBudget,
    AllowOrNot Children,
    AllowOrNot Visits,
    AllowOrNot OvernightGuests,
    AllowOrNot Smoking,
    AllowOrNot Pets,
    SleepSchedule SleepSchedule,
    SocialLevel SocialLevel,
    NoiseToleranceLevel NoiseToleranceLevel,
    string? Job,
    bool? IsStudent,
    string? University,
    string? Major
);
