using FluentValidation;
using Wanas.Application.DTOs.Listing;

namespace Wanas.Application.Validators.Listing
{
    public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentDtoValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Comment content is required.")
                .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");

            RuleFor(x => x.ListingId)
                .GreaterThan(0).WithMessage("Listing ID must be a positive number.");

            RuleFor(x => x.ParentCommentId)
                .GreaterThan(0).WithMessage("Parent comment ID must be positive if provided.")
                .When(x => x.ParentCommentId.HasValue);
        }
    }
}
