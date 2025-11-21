using FluentValidation;
using Wanas.Application.Commands.Admin;

namespace Wanas.Application.Validators
{
    public class BanUserCommandValidator : AbstractValidator<BanUserCommand>
  {
        public BanUserCommandValidator()
        {
            // Validate target user ID
            RuleFor(x => x.TargetUserId)
            .NotEmpty()
            .WithMessage("Target user ID is required.");
 
           // Validate admin ID
            RuleFor(x => x.AdminId)
            .NotEmpty()
            .WithMessage("Admin ID is required.");
     
           // Validate reason (mandatory for bans - more serious than suspension)
            RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required for banning a user.")
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters.");
        }
    }
}
