using FluentValidation;
using Wanas.Application.DTOs.Authentication;
namespace Wanas.Application.Validators.Authentication;
public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
