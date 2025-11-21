
namespace Wanas.Application.DTOs.Authentication;
public record RefreshTokenRequest(
    string Token,
    string RefreshToken
);
