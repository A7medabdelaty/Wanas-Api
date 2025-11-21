using FluentValidation;
using Wanas.Application.Commands.Admin;

namespace Wanas.Application.Validators
{
    public class UnverifyUserCommandValidator : AbstractValidator<UnverifyUserCommand>
    {
        public UnverifyUserCommandValidator()
        {
            RuleFor(x => x.TargetUserId)
               .NotEmpty()
               .WithMessage("Target user ID is required.");

            RuleFor(x => x.AdminId)
              .NotEmpty()
              .WithMessage("Admin ID is required.");

            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Reason))
                .WithMessage("Reason cannot exceed 500 characters.");
        }
    }
}
