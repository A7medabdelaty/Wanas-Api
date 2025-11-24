
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wanas.API.Extentions;
using Wanas.Application.DTOs.User;
using Wanas.Application.Interfaces;

namespace Wanas.API.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController(
    IUserService userService,
    ILogger<UserController> logger) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly ILogger<UserController> _logger = logger;

    //  Complete Profile
    [HttpPost("complete-profile")]
    public async Task<IActionResult> CompleteProfile(
        [FromBody] CompleteProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        _logger.LogInformation("User {UserId} attempting to complete profile", userId);

        var result = await _userService.CompleteProfileAsync(userId, request, cancellationToken);


        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    //  Skip Profile
    //[HttpPost("skip-profile")]
    //public async Task<IActionResult> SkipProfile(CancellationToken cancellationToken)
    //{
    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    //    if (string.IsNullOrEmpty(userId))
    //        return Unauthorized();

    //    _logger.LogInformation("User {UserId} skipping profile completion", userId);

    //    var result = await _userService.SkipProfileCompletionAsync(userId, cancellationToken);

    //    return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    //}

    // 3️⃣ Get Profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _userService.GetUserProfileAsync(userId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // 4️⃣ Update Profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        _logger.LogInformation("User {UserId} attempting to update profile", userId);

        var result = await _userService.UpdateProfileAsync(userId, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    // -----------------------------------------
    // PREFERENCES ENDPOINTS
    // -----------------------------------------

    
    // Complete Preferences - requires all preference fields
    
    [HttpPost("complete-preferences")]
    public async Task<IActionResult> CompletePreferences(
        [FromBody] CompletePreferencesRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        _logger.LogInformation("User {UserId} attempting to complete preferences", userId);

        var result = await _userService.CompletePreferencesAsync(userId, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    
    // Skip Preferences Completion
   
    //[HttpPost("skip-preferences")]
    //public async Task<IActionResult> SkipPreferences(CancellationToken cancellationToken)
    //{
    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    //    if (string.IsNullOrEmpty(userId))
    //        return Unauthorized();

    //    _logger.LogInformation("User {UserId} skipping preferences completion", userId);

    //    var result = await _userService.SkipPreferencesCompletionAsync(userId, cancellationToken);

    //    return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    //}

   
    // Get User Preferences
    
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _userService.GetUserPreferencesAsync(userId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

   
    // Update Preferences - all fields optional
    
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdatePreferencesRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        _logger.LogInformation("User {UserId} attempting to update preferences", userId);

        var result = await _userService.UpdatePreferencesAsync(userId, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}