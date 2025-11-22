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
                .When(x => !string.IsNullOrEmpty(x.Title));

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Description cannot exceed 2000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.City)
                .MaximumLength(100)
                .WithMessage("City cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.City));

            RuleFor(x => x.Address)
                .MaximumLength(200)
                .WithMessage("Address cannot exceed 200 characters.")
                .When(x => !string.IsNullOrEmpty(x.Address));

            RuleFor(x => x.MonthlyPrice)
                .GreaterThan(0)
                .WithMessage("Monthly price must be greater than zero.")
                .When(x => x.MonthlyPrice > 0);

            RuleFor(x => x.Floor)
                .MaximumLength(50)
                .WithMessage("Floor cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.Floor));

            RuleFor(x => x.AreaInSqMeters)
                .GreaterThan(0)
                .WithMessage("Area must be greater than 0 sq meters.")
                .When(x => x.AreaInSqMeters > 0);

            RuleFor(x => x.TotalRooms)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Total rooms must be at least 0.")
                .When(x => x.TotalRooms > 0);

            RuleFor(x => x.AvailableRooms)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Available rooms cannot be negative.")
                .LessThanOrEqualTo(x => x.TotalRooms)
                .WithMessage("Available rooms cannot exceed total rooms.")
                .When(x => x.AvailableRooms >= 0 && x.TotalRooms > 0);

            RuleFor(x => x.TotalBeds)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Total beds must be at least 0.")
                .When(x => x.TotalBeds > 0);

            RuleFor(x => x.AvailableBeds)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Available beds cannot be negative.")
                .LessThanOrEqualTo(x => x.TotalBeds)
                .WithMessage("Available beds cannot exceed total beds.")
                .When(x => x.AvailableBeds >= 0 && x.TotalBeds > 0);

            RuleFor(x => x.TotalBathrooms)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Total bathrooms must be at least 0.")
                .When(x => x.TotalBathrooms > 0);

            RuleForEach(x => x.NewPhotos)
                .ChildRules(photo =>
                {
                    photo.RuleFor(f => f.Length)
                        .LessThan(15 * 1024 * 1024)
                        .WithMessage("Each photo must be smaller than 15MB.");

                    photo.RuleFor(f => f.ContentType)
                        .Must(type => type == "image/jpeg" || 
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
