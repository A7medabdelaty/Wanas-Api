using FluentValidation;
using Wanas.Application.DTOs.Listing;

namespace Wanas.Application.Validators.Listing
{
    public class BedDtoValidator : AbstractValidator<BedDto>
    {
        public BedDtoValidator()
        {
            RuleFor(x => x.IsAvailable)
                .NotNull()
                .WithMessage("Bed availability must be specified.");
        }
    }

}
