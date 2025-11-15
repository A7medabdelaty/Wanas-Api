using FluentValidation;
using Wanas.Application.DTOs.Reports;

namespace Wanas.Application.Validators
{
    public class CreateReportDtoValidator : AbstractValidator<CreateReportDto>
    {


        public CreateReportDtoValidator()
        {
            RuleFor(x => x.TargetType)
                .IsInEnum()
                .WithMessage("Target type must be 'User' (0) or 'Listing' (1).");
            RuleFor(x => x.TargetId)
                .NotEmpty()
                .WithMessage("Target ID is required.");
            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("The reason for the report must be provided.")
                .Length(10, 500)
                .WithMessage("The reason must be between 10 and 500 characters long.");
            RuleForEach(x => x.PhotoUrls)
                .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .WithMessage("Each photo URL must be a valid absolute URL.");
                // .Must(urls => urls == null || urls.Count <= 5)
                //.WithMessage("A maximum of 5 photos can be attached to a report.");
        }
    }
}