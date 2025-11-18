using FluentValidation;
using Wanas.Application.DTOs.User;

namespace Wanas.Application.Validators.User;
public class CompleteProfileRequestValidator : AbstractValidator<CompleteProfileRequest>
{
    public CompleteProfileRequestValidator()
    {
        RuleFor(x => x.Age)
            .GreaterThan(0).WithMessage("Age must be greater than 0")
            .GreaterThanOrEqualTo(18).WithMessage("Age must be at least 18")
            .LessThanOrEqualTo(100).WithMessage("Age must not exceed 100");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));
    }
}