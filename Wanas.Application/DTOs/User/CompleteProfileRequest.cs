

using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.User;
public record CompleteProfileRequest(
    int Age,
    string PhoneNumber,
    string? Bio = null,
    string? Photo = null
);
