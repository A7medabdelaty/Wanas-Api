using FluentValidation;
using Wanas.Application.DTOs.Listing;

namespace Wanas.Application.Validators.Listing
{
    public class CreateListingDtoValidator : AbstractValidator<CreateListingDto>
    {
        public CreateListingDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(150).WithMessage("Title cannot exceed 150 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.");

            RuleFor(x => x.MonthlyPrice)
                .GreaterThan(0).WithMessage("Monthly price must be greater than zero.");

            RuleFor(x => x.Floor)
                .NotEmpty().WithMessage("Floor information is required.");

            RuleFor(x => x.AreaInSqMeters)
                .GreaterThan(0).WithMessage("Area must be greater than 0 sq meters.");

            RuleFor(x => x.TotalBathrooms)
                .GreaterThan(0).WithMessage("Total bathrooms must be greater than zero.");

            RuleFor(x => x.Photos)
                .NotNull().WithMessage("At least one photo is required.")
                .Must(p => p.Count > 0).WithMessage("At least one photo must be uploaded.");

            RuleFor(x => x.HasAirConditioner)
               .NotNull()
               .WithMessage("Air Conditioner must be specified.");

            RuleForEach(x => x.Photos).ChildRules(photo =>
            {
                photo.RuleFor(f => f.Length)
                    .LessThan(15 * 1024 * 1024) 
                    .WithMessage("Each photo must be smaller than 15MB.");

                photo.RuleFor(f => f.ContentType)
                    .Must(type =>
                        type == "image/jpeg" ||
                        type == "image/png" ||
                        type == "image/webp" ||
                        type == "image/jpg" ||
                        type == "image/heic")
                    .WithMessage("Only JPEG, JPG, PNG, WEBP and HEIC images are allowed.");
            });
        }
    }
}
