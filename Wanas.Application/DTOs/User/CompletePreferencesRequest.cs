
using Wanas.Domain.Enums;
namespace Wanas.Application.DTOs.User;

public record CompletePreferencesRequest(
    // Location & Demographics (Required)
    string City,
    int MinimumAge,
    int MaximumAge,
    Gender Gender,

    // Budget (Required)
    int MinimumBudget,
    int MaximumBudget,

    // Lifestyle (Required)
    AllowOrNot Children,
    AllowOrNot Visits,
    AllowOrNot OvernightGuests,
    AllowOrNot Smoking,
    AllowOrNot Pets,
    SleepSchedule SleepSchedule,
    SocialLevel SocialLevel,
    NoiseToleranceLevel NoiseToleranceLevel,

    // Work/Study (Optional)
    string? Job = null,
    bool? IsStudent = null,
    string? University = null,
    string? Major = null
);
