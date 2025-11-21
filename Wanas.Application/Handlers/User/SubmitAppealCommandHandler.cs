using MediatR;
using Microsoft.AspNetCore.Identity;
using Wanas.Application.Commands.User;
using Wanas.Application.Common;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Handlers.User
{
    public class SubmitAppealCommandHandler : IRequestHandler<SubmitAppealCommand, Result<Guid>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _db;

        public SubmitAppealCommandHandler(
            UserManager<ApplicationUser> userManager,
            AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<Result<Guid>> Handle(SubmitAppealCommand request, CancellationToken cancellationToken)
        {
            // Find the user
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return Result<Guid>.Failure("User not found");

            // Verify user is actually banned or suspended
            if (request.AppealType == AppealType.Ban && !user.IsBanned)
                return Result<Guid>.Failure("Cannot submit ban appeal - user is not banned");

            if (request.AppealType == AppealType.Suspension && !user.IsSuspended)
                return Result<Guid>.Failure("Cannot submit suspension appeal - user is not suspended");

            // Check if user already has a pending appeal of this type
            var existingAppeals = await _db.Appeals.FindAsync(a => 
                a.UserId == request.UserId && 
                a.AppealType == request.AppealType && 
                a.Status == AppealStatus.Pending);
            
            if (existingAppeals.Any())
                return Result<Guid>.Failure("You already have a pending appeal of this type");

            // Create appeal
            var appeal = new Appeal
            {
                UserId = request.UserId,
                AppealType = request.AppealType,
                Reason = request.Reason,
                Status = AppealStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // Save to database
            await _db.Appeals.AddAsync(appeal);
            await _db.CommitAsync();

            return Result<Guid>.Success(appeal.Id);
        }
    }
}
