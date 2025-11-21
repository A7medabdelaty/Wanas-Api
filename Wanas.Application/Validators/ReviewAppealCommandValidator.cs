using FluentValidation;
using Wanas.Application.Commands.Admin;

namespace Wanas.Application.Validators
{
    public class ReviewAppealCommandValidator : AbstractValidator<ReviewAppealCommand>
    {
        public ReviewAppealCommandValidator()
        {
            RuleFor(x => x.AppealId)
             .NotEmpty()
             .WithMessage("Appeal ID is required.");

            RuleFor(x => x.AdminId)
             .NotEmpty()
             .WithMessage("Admin ID is required.");

            RuleFor(x => x.AdminResponse)
              .MaximumLength(1000)
              .When(x => !string.IsNullOrEmpty(x.AdminResponse))
              .WithMessage("Admin response cannot exceed 1000 characters.");
        }
    }
}
