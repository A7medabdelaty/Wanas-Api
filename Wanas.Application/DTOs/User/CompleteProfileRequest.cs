using Microsoft.AspNetCore.Http;

namespace Wanas.Application.DTOs.User;
public record CompleteProfileRequest(
    int Age,
    string Bio,
    IFormFile? PhotoFile
);
