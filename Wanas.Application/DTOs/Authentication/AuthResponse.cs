
namespace Wanas.Application.DTOs.Authentication;
public record AuthResponse(
    string Id,
    string? Email,
    string FullName,
    string Token,
    int ExpiresIn,
    string RefreshToken,
    DateTime RefreshTokenExpiration,

    bool IsFirstLogin,
    bool IsProfileCompleted,
    bool IsPreferenceCompleted
);
