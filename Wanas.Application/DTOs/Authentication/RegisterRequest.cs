namespace Wanas.Application.DTOs.Authentication;
public record RegisterRequest(
    string Email,
    string Password,
    string FullName
);
