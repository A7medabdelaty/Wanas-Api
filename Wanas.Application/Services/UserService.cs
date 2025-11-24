using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

    //  COMPLETE PROFILE
    public async Task<Result<UserProfileResponse>> CompleteProfileAsync(
        string userId,
        CompleteProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || user.IsDeleted)
            return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

        // Update with required fields
        user.Age = request.Age;
        user.PhoneNumber = request.PhoneNumber;
        user.Bio = request.Bio;
        user.Photo = request.Photo;

        // Mark as completed
        user.IsProfileCompleted = true;
        user.IsFirstLogin = false;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure<UserProfileResponse>(
                new Error(error.Code, error.Description, StatusCodes.Status400BadRequest)
            );
        }

        _logger.LogInformation("User {UserId} completed profile", userId);

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
            user.IsFirstLogin,
            user.IsProfileCompleted,
            user.IsPreferenceCompleted
        );

        return Result.Success(response);
    }

    //  SKIP PROFILE
    //public async Task<Result<UserProfileResponse>> SkipProfileCompletionAsync(
    //    string userId,
    //    CancellationToken cancellationToken = default)
    //{
    //    var user = await _userManager.FindByIdAsync(userId);

    //    if (user is null || user.IsDeleted)
    //        return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

        
    //    user.IsFirstLogin = false;
       

    //    var result = await _userManager.UpdateAsync(user);

    //    if (!result.Succeeded)
    //    {
    //        var error = result.Errors.First();
    //        return Result.Failure<UserProfileResponse>(
    //            new Error(error.Code, error.Description, StatusCodes.Status400BadRequest)
    //        );
    //    }

    //    _logger.LogInformation("User {UserId} skipped profile completion", userId);


    //    var response = new UserProfileResponse(
    //        user.Id,
    //        user.Email!,
    //        user.FullName,
    //        user.ProfileType,
    //        user.Age,
    //        user.City,
    //        user.PhoneNumber,
    //        user.Bio,
    //        user.Photo,
    //        user.IsFirstLogin,
    //        user.IsProfileCompleted,
    //        user.IsPreferenceCompleted
    //    );

    //    return Result.Success(response);
    //}

    //  GET PROFILE
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
            user.IsFirstLogin,
            user.IsProfileCompleted,
            user.IsPreferenceCompleted
        );

        return Result.Success(response);
    }

    //  UPDATE PROFILE
    public async Task<Result<UserProfileResponse>> UpdateProfileAsync(
        string userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || user.IsDeleted)
            return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName;

        if (request.Age.HasValue)
            user.Age = request.Age.Value;

        if (!string.IsNullOrWhiteSpace(request.City))
            user.City = request.City;

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            user.PhoneNumber = request.PhoneNumber;

        if (request.Bio is not null)
            user.Bio = request.Bio;

        if (request.Photo is not null)
            user.Photo = request.Photo;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure<UserProfileResponse>(
                new Error(error.Code, error.Description, StatusCodes.Status400BadRequest)
            );
        }

        _logger.LogInformation("User {UserId} updated profile", userId);

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
            user.IsFirstLogin,
            user.IsProfileCompleted,
            user.IsPreferenceCompleted
        );

        return Result.Success(response);
    }
    // COMPLETE PREFERENCES
    // -----------------------------------------
    public async Task<Result<UserPreferencesResponse>> CompletePreferencesAsync(
        string userId,
        CompletePreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.UserPreference)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || user.IsDeleted)
            return Result.Failure<UserPreferencesResponse>(UserErrors.UserNotFound);

        // Check if preferences already exist
        if (user.UserPreference is not null)
            return Result.Failure<UserPreferencesResponse>(new Error(
                "PreferencesAlreadyExist",
                "User preferences already completed. Use update endpoint instead.",
                StatusCodes.Status400BadRequest
            ));

        // Create new preferences
        var preferences = new UserPreference
        {
            UserId = userId,
            City = request.City,
            MinimumAge = request.MinimumAge,
            MaximumAge = request.MaximumAge,
            Gender = request.Gender,
            MinimumBudget = request.MinimumBudget,
            MaximumBudget = request.MaximumBudget,
            Children = request.Children,
            Visits = request.Visits,
            OvernightGuests = request.OvernightGuests,
            Smoking = request.Smoking,
            Pets = request.Pets,
            SleepSchedule = request.SleepSchedule,
            SocialLevel = request.SocialLevel,
            NoiseToleranceLevel = request.NoiseToleranceLevel,
            Job = request.Job,
            IsStudent = request.IsStudent,
            University = request.University,
            Major = request.Major
        };

        user.UserPreference = preferences;
        user.IsPreferenceCompleted = true;
        user.IsFirstLogin = false;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure<UserPreferencesResponse>(
                new Error(error.Code, error.Description, StatusCodes.Status400BadRequest)
            );
        }

        _logger.LogInformation("User {UserId} completed preferences successfully", userId);

        var response = new UserPreferencesResponse(
            preferences.Id,
            preferences.City,
            preferences.MinimumAge,
            preferences.MaximumAge,
            preferences.Gender,
            preferences.MinimumBudget,
            preferences.MaximumBudget,
            preferences.Children,
            preferences.Visits,
            preferences.OvernightGuests,
            preferences.Smoking,
            preferences.Pets,
            preferences.SleepSchedule,
            preferences.SocialLevel,
            preferences.NoiseToleranceLevel,
            preferences.Job,
            preferences.IsStudent,
            preferences.University,
            preferences.Major
        );

        return Result.Success(response);
    }
    // SKIP PREFERENCES
    // -----------------------------------------
    //public async Task<Result<UserProfileResponse>> SkipPreferencesCompletionAsync(
    //    string userId,
    //    CancellationToken cancellationToken = default)
    //{
    //    var user = await _userManager.FindByIdAsync(userId);

    //    if (user is null || user.IsDeleted)
    //        return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

    //    // Mark first login as done
    //    user.IsFirstLogin = false;
    //    // IsPreferenceCompleted remains false

    //    var result = await _userManager.UpdateAsync(user);

    //    if (!result.Succeeded)
    //    {
    //        var error = result.Errors.First();
    //        return Result.Failure<UserProfileResponse>(
    //            new Error(error.Code, error.Description, StatusCodes.Status400BadRequest)
    //        );
    //    }

    //    _logger.LogInformation("User {UserId} skipped preferences completion", userId);

    //    // Return updated profile with flags
        
    //    var response = new UserProfileResponse(
    //        user.Id,
    //        user.Email!,
    //        user.FullName,
    //        user.ProfileType,
    //        user.Age,
    //        user.City,
    //        user.PhoneNumber,
    //        user.Bio,
    //        user.Photo,
    //        user.IsFirstLogin,
    //        user.IsProfileCompleted,
    //        user.IsPreferenceCompleted
    //    );

    //    return Result.Success(response);
    //}
    // GET PREFERENCES
    // -----------------------------------------
    public async Task<Result<UserPreferencesResponse>> GetUserPreferencesAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.UserPreference)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || user.IsDeleted)
            return Result.Failure<UserPreferencesResponse>(UserErrors.UserNotFound);

        if (user.UserPreference is null)
            return Result.Failure<UserPreferencesResponse>(new Error(
                "PreferencesNotFound",
                "User preferences not found",
                StatusCodes.Status404NotFound
            ));

        var response = new UserPreferencesResponse(
            user.UserPreference.Id,
            user.UserPreference.City,
            user.UserPreference.MinimumAge,
            user.UserPreference.MaximumAge,
            user.UserPreference.Gender,
            user.UserPreference.MinimumBudget,
            user.UserPreference.MaximumBudget,
            user.UserPreference.Children,
            user.UserPreference.Visits,
            user.UserPreference.OvernightGuests,
            user.UserPreference.Smoking,
            user.UserPreference.Pets,
            user.UserPreference.SleepSchedule,
            user.UserPreference.SocialLevel,
            user.UserPreference.NoiseToleranceLevel,
            user.UserPreference.Job,
            user.UserPreference.IsStudent,
            user.UserPreference.University,
            user.UserPreference.Major
        );

        return Result.Success(response);
    }
    // UPDATE PREFERENCES
    // -----------------------------------------
    public async Task<Result<UserPreferencesResponse>> UpdatePreferencesAsync(
        string userId,
        UpdatePreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.UserPreference)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || user.IsDeleted)
            return Result.Failure<UserPreferencesResponse>(UserErrors.UserNotFound);

        if (user.UserPreference is null)
            return Result.Failure<UserPreferencesResponse>(new Error(
                "PreferencesNotFound",
                "User preferences not found. Use complete endpoint first.",
                StatusCodes.Status404NotFound
            ));

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(request.City))
            user.UserPreference.City = request.City;

        if (request.MinimumAge.HasValue)
            user.UserPreference.MinimumAge = request.MinimumAge.Value;

        if (request.MaximumAge.HasValue)
            user.UserPreference.MaximumAge = request.MaximumAge.Value;

        if (request.Gender.HasValue)
            user.UserPreference.Gender = request.Gender.Value;

        if (request.MinimumBudget.HasValue)
            user.UserPreference.MinimumBudget = request.MinimumBudget.Value;

        if (request.MaximumBudget.HasValue)
            user.UserPreference.MaximumBudget = request.MaximumBudget.Value;

        if (request.Children.HasValue)
            user.UserPreference.Children = request.Children.Value;

        if (request.Visits.HasValue)
            user.UserPreference.Visits = request.Visits.Value;

        if (request.OvernightGuests.HasValue)
            user.UserPreference.OvernightGuests = request.OvernightGuests.Value;

        if (request.Smoking.HasValue)
            user.UserPreference.Smoking = request.Smoking.Value;

        if (request.Pets.HasValue)
            user.UserPreference.Pets = request.Pets.Value;

        if (request.SleepSchedule.HasValue)
            user.UserPreference.SleepSchedule = request.SleepSchedule.Value;

        if (request.SocialLevel.HasValue)
            user.UserPreference.SocialLevel = request.SocialLevel.Value;

        if (request.NoiseToleranceLevel.HasValue)
            user.UserPreference.NoiseToleranceLevel = request.NoiseToleranceLevel.Value;

        if (request.Job is not null)
            user.UserPreference.Job = request.Job;

        if (request.IsStudent.HasValue)
            user.UserPreference.IsStudent = request.IsStudent.Value;

        if (request.University is not null)
            user.UserPreference.University = request.University;

        if (request.Major is not null)
            user.UserPreference.Major = request.Major;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure<UserPreferencesResponse>(
                new Error(error.Code, error.Description, StatusCodes.Status400BadRequest)
            );
        }

        _logger.LogInformation("User {UserId} updated preferences successfully", userId);

        var response = new UserPreferencesResponse(
            user.UserPreference.Id,
            user.UserPreference.City,
            user.UserPreference.MinimumAge,
            user.UserPreference.MaximumAge,
            user.UserPreference.Gender,
            user.UserPreference.MinimumBudget,
            user.UserPreference.MaximumBudget,
            user.UserPreference.Children,
            user.UserPreference.Visits,
            user.UserPreference.OvernightGuests,
            user.UserPreference.Smoking,
            user.UserPreference.Pets,
            user.UserPreference.SleepSchedule,
            user.UserPreference.SocialLevel,
            user.UserPreference.NoiseToleranceLevel,
            user.UserPreference.Job,
            user.UserPreference.IsStudent,
            user.UserPreference.University,
            user.UserPreference.Major
        );

        return Result.Success(response);
    }

}
