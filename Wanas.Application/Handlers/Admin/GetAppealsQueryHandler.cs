using MediatR;
using Microsoft.AspNetCore.Identity;
using Wanas.Application.DTOs;
using Wanas.Application.Queries.Admin;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Handlers.Admin
{
    public class GetAppealsQueryHandler : IRequestHandler<GetAppealsQuery, IEnumerable<AppealDto>>
    {
        private readonly IUnitOfWork _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetAppealsQueryHandler(IUnitOfWork db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IEnumerable<AppealDto>> Handle(GetAppealsQuery request, CancellationToken cancellationToken)
        {
            // Get appeals based on status filter
            IEnumerable<Appeal> appeals;

            if (request.Status.HasValue)
            {
                // Filter by status
                appeals = await _db.Appeals.FindAsync(a => a.Status == request.Status.Value);
            }
            else
            {
                // Get all appeals
                appeals = await _db.Appeals.GetAllAsync();
            }

            // Map to DTOs with user information
            var appealDtos = new List<AppealDto>();

            foreach (var appeal in appeals.OrderByDescending(a => a.CreatedAt))
            {
                var user = await _userManager.FindByIdAsync(appeal.UserId);
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
