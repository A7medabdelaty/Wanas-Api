using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Wanas.Application.Abstractions;
using Wanas.Application.DTOs.User;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Errors;

namespace Wanas.Application.Services;
public class UserService(
    UserManager<ApplicationUser> userManager,
    ILogger<UserService> logger
) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ILogger<UserService> _logger = logger;

    // -----------------------------------------
    // COMPLETE PROFILE
    // -----------------------------------------
    public async Task<Result<UserProfileResponse>> CompleteProfileAsync(
        string userId,
        CompleteProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

        if (user.IsDeleted)
            return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

        // Update user profile
        user.ProfileType = request.ProfileType;
        user.Age = request.Age;
        user.City = request.City;
        user.PhoneNumber = request.PhoneNumber;
        user.Bio = request.Bio;
        user.Photo = request.Photo;

        // Mark profile as completed
        user.IsProfileCompleted = true;

        // Mark first login as done
        user.IsFirstLogin = false;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure<UserProfileResponse>(
                new Error(error.Code, error.Description, StatusCodes.Status400BadRequest)
            );
        }

        _logger.LogInformation("User {UserId} completed profile successfully", userId);

        // Return updated profile
        var response = new UserProfileResponse(
            user.Id,
            user.Email!,
            user.FullName,
            user.ProfileType,
            user.Age,
            user.City,
            user.PhoneNumber,
            user.Bio,
            user.Photo,
            user.IsProfileCompleted,
            user.IsPreferenceCompleted
        );

        return Result.Success(response);
    }

    // -----------------------------------------
    // GET USER PROFILE
    // -----------------------------------------
    public async Task<Result<UserProfileResponse>> GetUserProfileAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || user.IsDeleted)
            return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

        var response = new UserProfileResponse(
            user.Id,
            user.Email!,
            user.FullName,
            user.ProfileType,
            user.Age,
            user.City,
            user.PhoneNumber,
            user.Bio,
            user.Photo,
            user.IsProfileCompleted,
            user.IsPreferenceCompleted
        );

        return Result.Success(response);
    }
}
