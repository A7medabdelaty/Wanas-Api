
using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.Authentication;
public record RegisterRequest(
    string Email,
    string Password,
    string FullName,
    string City,              
    ProfileType ProfileType  
);