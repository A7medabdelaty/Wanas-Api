using MediatR;
using Microsoft.AspNetCore.Identity;
using Wanas.Application.DTOs;
using Wanas.Application.Queries.User;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Handlers.User
{
    public class GetMyAppealsQueryHandler : IRequestHandler<GetMyAppealsQuery, IEnumerable<AppealDto>>
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetMyAppealsQueryHandler(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IEnumerable<AppealDto>> Handle(GetMyAppealsQuery request, CancellationToken cancellationToken)
        {
            // Get current user's appeals
            var appeals = await _db.Appeals.FindAsync(a => a.UserId == request.UserId);

            // Get user info
            var user = await _userManager.FindByIdAsync(request.UserId);

            // Map to DTOs
            var appealDtos = new List<AppealDto>();

            foreach (var appeal in appeals.OrderByDescending(a => a.CreatedAt))
            {
                ApplicationUser? reviewedByAdmin = null;

                if (!string.IsNullOrEmpty(appeal.ReviewedByAdminId))
                {
                    reviewedByAdmin = await _userManager.FindByIdAsync(appeal.ReviewedByAdminId);
                }

                appealDtos.Add(new AppealDto
                {
                    Id = appeal.Id,
                    UserId = appeal.UserId,
                    UserFullName = user?.FullName ?? "Unknown User",
                    UserEmail = user?.Email ?? "No Email",
                    AppealType = appeal.AppealType,
                    AppealTypeText = appeal.AppealType == AppealType.Ban ? "Ban" : "Suspension",
                    Reason = appeal.Reason,
                    Status = appeal.Status,
                    StatusText = appeal.Status.ToString(),
                    ReviewedByAdminId = appeal.ReviewedByAdminId,
                    ReviewedByAdminName = reviewedByAdmin?.FullName,
                    AdminResponse = appeal.AdminResponse,
                    CreatedAt = appeal.CreatedAt,
                    ReviewedAt = appeal.ReviewedAt
                });
            }

            return appealDtos;
        }
    }
}
