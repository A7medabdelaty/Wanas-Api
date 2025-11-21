using Microsoft.AspNetCore.Authorization;

namespace Wanas.API.Authorization
{
    /// <summary>
    /// Requirement that user must be verified
    /// </summary>
    public class VerifiedUserRequirement : IAuthorizationRequirement
    {
    }
}
