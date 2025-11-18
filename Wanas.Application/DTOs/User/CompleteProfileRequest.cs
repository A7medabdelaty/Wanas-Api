

using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.User;
public record CompleteProfileRequest(
    ProfileType ProfileType,
    int Age,
    string City,
    string PhoneNumber,
    string? Bio = null,
    string? Photo = null
);
