using FluentValidation;
using Wanas.Application.DTOs.Listing;

namespace Wanas.Application.Validators.Listing
{
    public class UpdateListingDtoValidator : AbstractValidator<UpdateListingDto>
    {
        public UpdateListingDtoValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(150)
                .WithMessage("Title cannot exceed 150 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Title));

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Description cannot exceed 2000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.City)
                .MaximumLength(100)
                .WithMessage("City cannot exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.City));

            RuleFor(x => x.Address)
                .MaximumLength(200)
                .WithMessage("Address cannot exceed 200 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Address));

            RuleFor(x => x.MonthlyPrice)
                .GreaterThan(0)
                .WithMessage("Monthly price must be greater than zero.");

            RuleFor(x => x.Floor)
                .MaximumLength(50)
                .WithMessage("Floor cannot exceed 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Floor));

            RuleFor(x => x.AreaInSqMeters)
                .GreaterThan(0)
                .WithMessage("Area must be greater than 0 sq meters.")
                .When(x => x.AreaInSqMeters > 0);

            RuleFor(x => x.TotalBathrooms)
                .GreaterThan(0)
                .WithMessage("Total bathrooms must be greater than zero.")
                .When(x => x.TotalBathrooms != default);

            RuleForEach(x => x.NewPhotos)
                .ChildRules(photo =>
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
                })
                .When(x => x.NewPhotos != null && x.NewPhotos.Count > 0);
        }
    }

}
