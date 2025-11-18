using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.User;
public record UserProfileResponse(
    string Id,
    string Email,
    string FullName,
    ProfileType? ProfileType,
    int? Age,
    string? City,
    string? PhoneNumber,
    string? Bio,
    string? Photo,
    bool IsFirstLogin,
    bool IsProfileCompleted,
    bool IsPreferenceCompleted
);