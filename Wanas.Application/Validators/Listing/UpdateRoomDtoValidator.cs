using FluentValidation;
using Wanas.Application.DTOs.Listing;

namespace Wanas.Application.Validators.Listing
{
    public class UpdateRoomDtoValidator : AbstractValidator<UpdateRoomDto>
    {
        public UpdateRoomDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Room ID must be a positive number.");

            RuleFor(x => x.RoomNumber)
                .GreaterThan(0)
                .WithMessage("Room number must be a positive number.")
                .When(x => x.RoomNumber > 0); // only validate if provided

            RuleFor(x => x.BedsCount)
                .GreaterThan(0)
                .WithMessage("BedsCount must be greater than zero.")
                .When(x => x.BedsCount.HasValue);

            RuleFor(x => x.AvailableBeds)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Available beds cannot be negative.")
                .When(x => x.AvailableBeds.HasValue);

            RuleFor(x => x.HasAirConditioner)
                .NotNull()
                .WithMessage("Air Conditioner availability must be specified.");

            RuleFor(x => x)
                .Must(x =>
                    !x.BedsCount.HasValue ||
                    !x.AvailableBeds.HasValue ||
                    x.AvailableBeds <= x.BedsCount)
                .WithMessage("Available beds cannot exceed total beds.")
                .When(x => x.BedsCount.HasValue && x.AvailableBeds.HasValue);

            RuleFor(x => x.PricePerBed)
                .GreaterThan(0)
                .WithMessage("Price per bed must be greater than zero.")
                .When(x => x.PricePerBed.HasValue);
        }
    }

}
