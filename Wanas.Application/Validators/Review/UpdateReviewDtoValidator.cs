using FluentValidation;
using Wanas.Application.DTOs.Review;

namespace Wanas.Application.Validators.Review
{
    public class UpdateReviewDtoValidator : AbstractValidator<UpdateReviewDto>
    {
        public UpdateReviewDtoValidator()
        {
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
