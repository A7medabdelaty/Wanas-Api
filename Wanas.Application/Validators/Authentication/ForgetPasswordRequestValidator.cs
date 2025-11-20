
using FluentValidation;
using Wanas.Application.DTOs.Authentication;

namespace Wanas.Application.Validators.Authentication;
public class ForgetPasswordRequestValidator : AbstractValidator<ForgetPasswordRequest>
{
    public ForgetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
