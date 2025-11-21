using FluentValidation;
using Wanas.Application.Commands.User;

namespace Wanas.Application.Validators
{
    public class SubmitAppealCommandValidator : AbstractValidator<SubmitAppealCommand>
    {
        public SubmitAppealCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");

            RuleFor(x => x.AppealType)
                .IsInEnum()
                .WithMessage("Invalid appeal type.");

            RuleFor(x => x.Reason)
              .NotEmpty()
              .WithMessage("Reason is required for submitting an appeal.")
              .MinimumLength(50)
              .WithMessage("Reason must be at least 50 characters.")
              .MaximumLength(2000)
              .WithMessage("Reason cannot exceed 2000 characters.");
        }
    }
}
