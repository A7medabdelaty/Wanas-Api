using FluentValidation;
using Wanas.Application.Commands.Admin;

namespace Wanas.Application.Validators
{
    public class SuspendUserCommandValidator : AbstractValidator<SuspendUserCommand>
    {
        public SuspendUserCommandValidator()
        {
            RuleFor(x => x.TargetUserId)
                .NotEmpty()
                .WithMessage("Target user ID is required.");

            RuleFor(x => x.AdminId)
                  .NotEmpty()
                  .WithMessage("Admin ID is required.");

            RuleFor(x => x.DurationDays)
                  .Must(d => d == null || d > 0)
                  .WithMessage("Duration must be null (indefinite) or a positive number.")
                  .LessThanOrEqualTo(365 * 10)  // Max 10 years
                  .WithMessage("Duration cannot exceed 10 years.");

            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Reason))
                .WithMessage("Reason cannot exceed 500 characters.");

        }
    }
}
