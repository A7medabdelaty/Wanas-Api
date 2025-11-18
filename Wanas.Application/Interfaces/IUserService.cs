
using Wanas.Application.Abstractions;
using Wanas.Application.DTOs.User;

namespace Wanas.Application.Interfaces;

    public interface IUserService
    {
        Task<Result<UserProfileResponse>> CompleteProfileAsync(string userId, CompleteProfileRequest request, CancellationToken cancellationToken = default);
        Task<Result<UserProfileResponse>> SkipProfileCompletionAsync(string userId, CancellationToken cancellationToken = default);
        Task<Result<UserProfileResponse>> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default);
        Task<Result<UserProfileResponse>> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
        Task<Result<UserPreferencesResponse>> CompletePreferencesAsync(string userId, CompletePreferencesRequest request, CancellationToken cancellationToken = default);
        Task<Result<UserProfileResponse>> SkipPreferencesCompletionAsync(string userId, CancellationToken cancellationToken = default);
        Task<Result<UserPreferencesResponse>> GetUserPreferencesAsync(string userId, CancellationToken cancellationToken = default);
        Task<Result<UserPreferencesResponse>> UpdatePreferencesAsync(string userId, UpdatePreferencesRequest request, CancellationToken cancellationToken = default);
}

