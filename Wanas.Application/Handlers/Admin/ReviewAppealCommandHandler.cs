using MediatR;
using Microsoft.AspNetCore.Identity;
using Wanas.Application.Commands.Admin;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Handlers.Admin
{
    public class ReviewAppealCommandHandler : IRequestHandler<ReviewAppealCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _db;
        private readonly IAuditLogService _audit;
        private readonly IMediator _mediator;

        public ReviewAppealCommandHandler(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork db,
            IAuditLogService audit,
            IMediator mediator)
        {
            _userManager = userManager;
            _db = db;
            _audit = audit;
            _mediator = mediator;
        }

        public async Task<bool> Handle(ReviewAppealCommand request, CancellationToken cancellationToken)
        {
            // Find the appeal
            var appeals = await _db.Appeals.FindAsync(a => a.Id == request.AppealId);
            var appeal = appeals.FirstOrDefault();
            if (appeal == null) return false;

            // Check if appeal is already reviewed
            if (appeal.Status != AppealStatus.Pending)
                return false;

            // Update appeal status
            appeal.Status = request.IsApproved ? AppealStatus.Approved : AppealStatus.Rejected;
            appeal.ReviewedByAdminId = request.AdminId;
            appeal.AdminResponse = request.AdminResponse;
            appeal.ReviewedAt = DateTime.UtcNow;

            // If approved, lift the ban/suspension
            if (request.IsApproved)
            {
                var user = await _userManager.FindByIdAsync(appeal.UserId);
                if (user != null)
                {
                    if (appeal.AppealType == AppealType.Ban && user.IsBanned)
                    {
                        // Unban the user
                        var unbanCommand = new UnbanUserCommand(
                            user.Id,
                            request.AdminId,
                            $"Appeal approved: {appeal.Id}"
                        );
                        await _mediator.Send(unbanCommand, cancellationToken);
                    }
                    else if (appeal.AppealType == AppealType.Suspension && user.IsSuspended)
                    {
                        // Unsuspend the user
                        var unsuspendCommand = new UnsuspendUserCommand(
                            user.Id,
                            request.AdminId,
                            $"Appeal approved: {appeal.Id}"
                        );
                        await _mediator.Send(unsuspendCommand, cancellationToken);
                    }
                }
            }

            // Save changes
            await _db.CommitAsync();

            // Log the action
            var details = $"AppealId={appeal.Id}; Status={appeal.Status}; Response={request.AdminResponse}";
            await _audit.LogAsync("ReviewAppeal", request.AdminId, appeal.UserId, details);

            return true;
        }
    }
}
