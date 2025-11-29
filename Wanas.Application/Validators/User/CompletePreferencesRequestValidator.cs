using FluentValidation;
using Wanas.Application.DTOs.User;

namespace Wanas.Application.Validators.User;
public class CompletePreferencesRequestValidator : AbstractValidator<CompletePreferencesRequest>
{
    public CompletePreferencesRequestValidator()
    {
        // City
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MinimumLength(2).WithMessage("City must be at least 2 characters")
            .MaximumLength(50).WithMessage("City must not exceed 50 characters");

        // Age Range
        RuleFor(x => x.MinimumAge)
            .GreaterThanOrEqualTo(18).WithMessage("Minimum age must be at least 18")
            .LessThanOrEqualTo(100).WithMessage("Minimum age must not exceed 100");

        RuleFor(x => x.MaximumAge)
            .GreaterThanOrEqualTo(18).WithMessage("Maximum age must be at least 18")
            .LessThanOrEqualTo(100).WithMessage("Maximum age must not exceed 100");

        RuleFor(x => x)
            .Must(x => x.MinimumAge <= x.MaximumAge)
            .WithMessage("Minimum age must be less than or equal to maximum age")
            .WithName("AgeRange");

        // Budget Range
        RuleFor(x => x.MinimumBudget)
            .GreaterThan(0).WithMessage("Minimum budget must be greater than 0");

        RuleFor(x => x.MaximumBudget)
            .GreaterThan(0).WithMessage("Maximum budget must be greater than 0");

        RuleFor(x => x)
            .Must(x => x.MinimumBudget <= x.MaximumBudget)
            .WithMessage("Minimum budget must be less than or equal to maximum budget")
            .WithName("BudgetRange");

        // Gender
        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender value");

        // Lifestyle Preferences
        RuleFor(x => x.Children)
            .IsInEnum().WithMessage("Invalid value for children preference");

        RuleFor(x => x.Visits)
            .IsInEnum().WithMessage("Invalid value for visits preference");

        RuleFor(x => x.OvernightGuests)
            .IsInEnum().WithMessage("Invalid value for overnight guests preference");

        RuleFor(x => x.Smoking)
            .IsInEnum().WithMessage("Invalid value for smoking preference");

        RuleFor(x => x.Pets)
            .IsInEnum().WithMessage("Invalid value for pets preference");

        RuleFor(x => x.SleepSchedule)
            .IsInEnum().WithMessage("Invalid sleep schedule value");

        RuleFor(x => x.SocialLevel)
            .IsInEnum().WithMessage("Invalid social level value");

        RuleFor(x => x.NoiseToleranceLevel)
            .IsInEnum().WithMessage("Invalid noise tolerance level value");

        // Optional Fields
        RuleFor(x => x.Job)
            .MaximumLength(100).WithMessage("Job must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Job));

        RuleFor(x => x.University)
            .MaximumLength(100).WithMessage("University must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.University));

        RuleFor(x => x.Major)
            .MaximumLength(100).WithMessage("Major must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Major));
    }
}
