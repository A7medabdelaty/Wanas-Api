using FluentValidation;
using Wanas.Application.DTOs.User;

namespace Wanas.Application.Validators.User;
public class UpdatePreferencesRequestValidator : AbstractValidator<UpdatePreferencesRequest>
{
    public UpdatePreferencesRequestValidator()
    {
        // City
        RuleFor(x => x.City)
            .MinimumLength(2).WithMessage("City must be at least 2 characters")
            .MaximumLength(50).WithMessage("City must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        // Age Range
        RuleFor(x => x.MinimumAge)
            .GreaterThanOrEqualTo(18).WithMessage("Minimum age must be at least 18")
            .LessThanOrEqualTo(100).WithMessage("Minimum age must not exceed 100")
            .When(x => x.MinimumAge.HasValue);

        RuleFor(x => x.MaximumAge)
            .GreaterThanOrEqualTo(18).WithMessage("Maximum age must be at least 18")
            .LessThanOrEqualTo(100).WithMessage("Maximum age must not exceed 100")
            .When(x => x.MaximumAge.HasValue);

        // Budget Range
        RuleFor(x => x.MinimumBudget)
            .GreaterThan(0).WithMessage("Minimum budget must be greater than 0")
            .When(x => x.MinimumBudget.HasValue);

        RuleFor(x => x.MaximumBudget)
            .GreaterThan(0).WithMessage("Maximum budget must be greater than 0")
            .When(x => x.MaximumBudget.HasValue);

        // Gender
        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender value")
            .When(x => x.Gender.HasValue);

        // Lifestyle Preferences
        RuleFor(x => x.Children)
            .IsInEnum().WithMessage("Invalid value for children preference")
            .When(x => x.Children.HasValue);

        RuleFor(x => x.Visits)
            .IsInEnum().WithMessage("Invalid value for visits preference")
            .When(x => x.Visits.HasValue);

        RuleFor(x => x.OvernightGuests)
            .IsInEnum().WithMessage("Invalid value for overnight guests preference")
            .When(x => x.OvernightGuests.HasValue);

        RuleFor(x => x.Smoking)
            .IsInEnum().WithMessage("Invalid value for smoking preference")
            .When(x => x.Smoking.HasValue);

        RuleFor(x => x.Pets)
            .IsInEnum().WithMessage("Invalid value for pets preference")
            .When(x => x.Pets.HasValue);

        RuleFor(x => x.SleepSchedule)
            .IsInEnum().WithMessage("Invalid sleep schedule value")
            .When(x => x.SleepSchedule.HasValue);

        RuleFor(x => x.SocialLevel)
            .IsInEnum().WithMessage("Invalid social level value")
            .When(x => x.SocialLevel.HasValue);

        RuleFor(x => x.NoiseToleranceLevel)
            .IsInEnum().WithMessage("Invalid noise tolerance level value")
            .When(x => x.NoiseToleranceLevel.HasValue);

        // Optional Fields
        RuleFor(x => x.Job)
            .MaximumLength(100).WithMessage("Job must not exceed 100 characters")
            .When(x => x.Job is not null);

        RuleFor(x => x.University)
            .MaximumLength(100).WithMessage("University must not exceed 100 characters")
            .When(x => x.University is not null);

        RuleFor(x => x.Major)
            .MaximumLength(100).WithMessage("Major must not exceed 100 characters")
            .When(x => x.Major is not null);
    }
}
