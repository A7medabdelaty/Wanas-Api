using FluentValidation;
using Wanas.Application.DTOs.Review;

namespace Wanas.Application.Validators.Review
{
    public class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
    {
        public CreateReviewDtoValidator()
        {
            RuleFor(x => x.TargetType)
                .IsInEnum()
                .WithMessage("Target type must be 'User' (0) or 'Listing' (1).");

            RuleFor(x => x.TargetId)
                .NotEmpty()
                .WithMessage("Target ID is required.");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5.");

            RuleFor(x => x.Comment)
                .NotEmpty()
                .WithMessage("Comment is required.")
                .MaximumLength(2000)
                .WithMessage("Comment cannot exceed 2000 characters.");
        }
    }
}
