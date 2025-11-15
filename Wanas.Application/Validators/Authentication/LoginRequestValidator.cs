using FluentValidation;
using Wanas.Application.DTOs.Authentication;

namespace Wanas.Application.Validators.Authentication;
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}