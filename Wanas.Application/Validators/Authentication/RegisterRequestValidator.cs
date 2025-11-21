
using FluentValidation;
using Wanas.Application.Abstractions;
using Wanas.Application.DTOs.Authentication;
using Wanas.Domain.Enums;

namespace Wanas.Application.Validators.Authentication;
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .Matches(RegexPatterns.Password)
            .WithMessage("Password should be at least 8 digits and should contains Lowercase, NonAlphanumeric and Uppercase");



        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MinimumLength(3).WithMessage("Full name must be at least 3 characters")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MinimumLength(2).WithMessage("City must be at least 2 characters")
            .MaximumLength(50).WithMessage("City must not exceed 50 characters");

        RuleFor(x => x.ProfileType)
            .NotNull().WithMessage("Profile type is required")
            .Must(pt => pt == ProfileType.Owner || pt == ProfileType.Renter)
            .WithMessage("Profile type must be Owner or Renter only");

    }
}
