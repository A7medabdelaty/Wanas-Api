using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanas.Application.DTOs.User;
public record UpdateProfileRequest(
    string? FullName = null,
    int? Age = null,
    string? City = null,
    string? PhoneNumber = null,
    string? Bio = null,
    string? Photo = null
);
