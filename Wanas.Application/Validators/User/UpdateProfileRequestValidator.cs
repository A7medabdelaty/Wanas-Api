
using FluentValidation;
using Wanas.Application.DTOs.User;

namespace Wanas.Application.Validators.User;
public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .MinimumLength(3).WithMessage("Full name must be at least 3 characters")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FullName));

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18).WithMessage("Age must be at least 18")
            .LessThanOrEqualTo(100).WithMessage("Age must not exceed 100")
            .When(x => x.Age.HasValue);

        RuleFor(x => x.City)
            .MinimumLength(2).WithMessage("City must be at least 2 characters")
            .MaximumLength(50).WithMessage("City must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio must not exceed 500 characters")
            .When(x => x.Bio is not null);

    }
}