
using Wanas.Application.Abstractions;
using Wanas.Application.DTOs.User;

namespace Wanas.Application.Interfaces;
public interface IUserService
{
    Task<Result<UserProfileResponse>> CompleteProfileAsync(string userId, CompleteProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result<UserProfileResponse>> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default);
}
