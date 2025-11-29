
using Microsoft.AspNetCore.Http;

namespace Wanas.Application.DTOs.User;
public record UpdateProfileRequest(
    string? FullName = null,
    int? Age = null,
    string? City = null,
    string? PhoneNumber = null,
    string? Bio = null,
    IFormFile? PhotoFile=null
);
