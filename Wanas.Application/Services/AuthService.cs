using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Security.Cryptography;
using System.Text;
using Wanas.Application.Abstractions;
using Wanas.Application.DTOs.Authentication;
using Wanas.Application.Helpers;
using Wanas.Application.Interfaces;
using Wanas.Application.Interfaces.Authentication;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Errors;

namespace Wanas.Application.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtProvider jwtProvider,
    ILogger<AuthService> logger,
    IEmailService emailSender,
    IHttpContextAccessor httpContextAccessor
) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IEmailService _emailSender = emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    private readonly int _refreshTokenExpiryDays = 14;

    // -----------------------------------------
    // LOGIN
    // -----------------------------------------
    public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

        if (!result.Succeeded)
            return Result.Failure<AuthResponse>(result.IsNotAllowed ? UserErrors.EmailNotConfirmed : UserErrors.InvalidCredentials);

        var roles = await _userManager.GetRolesAsync(user);

        var (token, expiresIn) = _jwtProvider.GenerateToken(user, roles);

        var refreshToken = GenerateRefreshToken();
        var refreshExpires = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresOn = refreshExpires
        });

        await _userManager.UpdateAsync(user);

        return Result.Success(new AuthResponse(
            user.Id,
            user.Email!,
            user.FullName,
            token,
            expiresIn,
            refreshToken,
            refreshExpires,
            user.IsFirstLogin,
            user.IsProfileCompleted,
            user.IsPreferenceCompleted
        ));
    }

    // -----------------------------------------
    // REFRESH TOKEN
    // -----------------------------------------
    public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);
        if (userId is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

        var savedToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);
        if (savedToken is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);

        savedToken.RevokedOn = DateTime.UtcNow;

        var roles = await _userManager.GetRolesAsync(user);

        var (newJwt, expiresIn) = _jwtProvider.GenerateToken(user, roles);

        var newRefresh = GenerateRefreshToken();
        var newRefreshExpires = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefresh,
            ExpiresOn = newRefreshExpires
        });

        await _userManager.UpdateAsync(user);

        return Result.Success(new AuthResponse(
            user.Id,
            user.Email!,
            user.FullName,
            newJwt,
            expiresIn,
            newRefresh,
            newRefreshExpires,
            user.IsFirstLogin,
            user.IsProfileCompleted,
            user.IsPreferenceCompleted
        ));
    }

    // -----------------------------------------
    // REVOKE REFRESH TOKEN
    // -----------------------------------------
    public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);
        if (userId is null)
            return Result.Failure(UserErrors.InvalidJwtToken);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(UserErrors.InvalidJwtToken);

        var saved = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);
        if (saved is null)
            return Result.Failure(UserErrors.InvalidRefreshToken);

        saved.RevokedOn = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Result.Success();
    }

    // -----------------------------------------
    // REGISTER
    // -----------------------------------------
    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {

        if (request.ProfileType == ProfileType.Admin)
            return Result.Failure(UserErrors.InvalidProfileType);


        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            return Result.Failure(UserErrors.DuplicatedEmail);

        var user = request.Adapt<ApplicationUser>();
        var userName = request.Email.Replace("@", "").Replace(".", "");
        user.UserName = userName;
        user.NormalizedUserName = userName.ToUpper();
        user.Email = request.Email;
        user.NormalizedEmail = request.Email.ToUpper();
        

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var e = result.Errors.First();
            return Result.Failure(new Error(e.Code, e.Description, StatusCodes.Status400BadRequest));
        }
        var roleName = request.ProfileType == ProfileType.Owner ? "Owner" : "Renter";
        var roleResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user); // rollback user creation
            var e = roleResult.Errors.First();
            return Result.Failure(new Error(e.Code, e.Description, StatusCodes.Status400BadRequest));
        }



        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        await SendConfirmationEmail(user, code);

        return Result.Success();
    }

    // -----------------------------------------
    // CONFIRM EMAIL
    // -----------------------------------------
    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return Result.Failure(UserErrors.InvalidCode);

        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.DuplicatedConfirmation);

        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
        }
        catch
        {
            return Result.Failure(UserErrors.InvalidCode);
        }

        var result = await _userManager.ConfirmEmailAsync(user, decoded);

        if (!result.Succeeded)
        {
            var e = result.Errors.First();
            return Result.Failure(new Error(e.Code, e.Description, StatusCodes.Status400BadRequest));
        }

        return Result.Success();
    }

    // -----------------------------------------
    // RESEND CONFIRM EMAIL
    // -----------------------------------------
    public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return Result.Success(); // don't leak info

        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.DuplicatedConfirmation);

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        await SendConfirmationEmail(user, code);

        return Result.Success();
    }

    // -----------------------------------------
    // FORGET PASSWORD
    // -----------------------------------------
    public async Task<Result> SendResetPasswordCodeAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Result.Success();

        if (!user.EmailConfirmed)
            return Result.Failure(UserErrors.EmailNotConfirmed);

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        await SendResetPasswordEmail(user, code);

        return Result.Success();
    }

    // -----------------------------------------
    // RESET PASSWORD
    // -----------------------------------------
    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.EmailConfirmed)
            return Result.Failure(UserErrors.InvalidCode);

        IdentityResult result;

        try
        {
            var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            result = await _userManager.ResetPasswordAsync(user, decoded, request.NewPassword);
        }
        catch
        {
            result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
        }

        if (!result.Succeeded)
        {
            var e = result.Errors.First();
            return Result.Failure(new Error(e.Code, e.Description, StatusCodes.Status401Unauthorized));
        }

        return Result.Success();
    }

    // -----------------------------------------
    // EMAIL HELPERS
    // -----------------------------------------
    private async Task SendConfirmationEmail(ApplicationUser user, string code)
    {
        var originHeader = _httpContextAccessor.HttpContext?.Request.Headers["Origin"];
        var origin = originHeader.HasValue && !StringValues.IsNullOrEmpty(originHeader.Value) ? originHeader.ToString() : null;

        var body = EmailBodyBuilder.GenerateEmailBody(
            "EmailConfirmation",
            new()
            {
                { "{{name}}", user.FullName },
                { "{{action_url}}", $"{origin}/auth/emailConfirmation?userId={user.Id}&code={code}" }
            });

        await _emailSender.SendEmailAsync(user.Email!, "Email Confirmation", body);
    }

    private async Task SendResetPasswordEmail(ApplicationUser user, string code)
    {
        var originHeader = _httpContextAccessor.HttpContext?.Request.Headers["Origin"];
        var origin = originHeader.HasValue && !StringValues.IsNullOrEmpty(originHeader.Value) ? originHeader.ToString() : null;

        var body = EmailBodyBuilder.GenerateEmailBody(
            "ForgetPassword",
            new()
            {
                { "{{name}}", user.FullName },
                { "{{action_url}}", $"{origin}/auth/forgetPassword?email={user.Email}&code={code}" }
            });

        await _emailSender.SendEmailAsync(user.Email!, "Reset Password", body);
    }

    
    private static string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
