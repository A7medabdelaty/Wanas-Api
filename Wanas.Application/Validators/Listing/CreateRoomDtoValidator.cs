using FluentValidation;
using Wanas.Application.DTOs.Listing;

namespace Wanas.Application.Validators.Listing
{
    public class CreateRoomDtoValidator : AbstractValidator<CreateRoomDto>
    {
        public CreateRoomDtoValidator()
        {
            RuleFor(x => x.RoomNumber)
                .GreaterThan(0)
                .WithMessage("Room number must be a positive number.");

            RuleFor(x => x.BedsCount)
                .GreaterThan(0)
                .WithMessage("BedsCount must be greater than zero.");

            RuleFor(x => x.AvailableBeds)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Available beds cannot be negative.")
                .LessThanOrEqualTo(x => x.BedsCount)
                .WithMessage("Available beds cannot exceed total beds.");

            RuleFor(x => x.PricePerBed)
                .GreaterThan(0)
                .WithMessage("Price per bed must be greater than zero.");

            RuleFor(x => x.HasAirConditioner)
                .NotNull()
                .WithMessage("Air Conditioner must be specified.");

            // Validate beds if provided
            RuleForEach(x => x.Beds).SetValidator(new BedDtoValidator());

            RuleFor(x => x.Beds)
                .Must((dto, beds) => beds == null || beds.Count == dto.BedsCount)
                .WithMessage("If beds are provided, their count must match BedsCount.");
        }
    }

}
