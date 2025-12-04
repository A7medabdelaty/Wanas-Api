//using Microsoft.EntityFrameworkCore;
//using Wanas.Application.DTOs.Analytics;
//using Wanas.Application.Interfaces;
//using Wanas.Domain.Enums;
//using Wanas.Domain.Repositories;

//namespace Wanas.Application.Services
//{
//    public class AnalyticsService : IAnalyticsService
//    {
//        private readonly IUnitOfWork _uow;
//        public AnalyticsService(IUnitOfWork uow) { _uow = uow; }
//        public async Task<AnalyticsSummaryDto> GetSummaryAsync(DateOnly date)
//        {
//            var listings = await _uow.Listings.GetAllAsync();
//            var trafficCount = (await _uow.TrafficLogs.FindAsync(t => t.CreatedAt.Date == date.ToDateTime(TimeOnly.MinValue).Date)).Count();
//            var activeUsers = (await _uow.TrafficLogs.FindAsync(t => t.CreatedAt.Date == date.ToDateTime(TimeOnly.MinValue).Date && t.UserId != null)).Select(t => t.UserId).Distinct().Count();
//            return new AnalyticsSummaryDto
//            {
//                Date = date,
//                TotalListings = listings.Count(),
//                PendingListings = listings.Count(l => l.ModerationStatus == ListingModerationStatus.Pending),
//                ApprovedListings = listings.Count(l => l.ModerationStatus == ListingModerationStatus.Approved),
//                RejectedListings = listings.Count(l => l.ModerationStatus == ListingModerationStatus.Rejected),
//                FlaggedListings = listings.Count(l => l.IsFlagged),
//                ActiveUsers = activeUsers,
//                Requests = trafficCount
//            };
//        }
//        public async Task<IEnumerable<TrafficPointDto>> GetTrafficAsync(DateOnly from, DateOnly to)
//        {
//            var start = from.ToDateTime(TimeOnly.MinValue);
//            var end = to.ToDateTime(TimeOnly.MaxValue);
//            var logs = await _uow.TrafficLogs.FindAsync(t => t.CreatedAt >= start && t.CreatedAt <= end);
//            return logs.Select(l => new TrafficPointDto { Timestamp = l.CreatedAt, Path = l.Path, Method = l.Method, Authenticated = l.UserId != null });
//        }
//        public async Task<ModerationKpiDto> GetModerationKpisAsync(DateOnly date)
//        {
//            var dayListings = (await _uow.Listings.GetAllAsync());
//            var approved = dayListings.Where(l => l.ModerationStatus == ListingModerationStatus.Approved);
//            var approvalTimes = approved.Where(l => l.ModeratedAt.HasValue).Select(l => (l.ModeratedAt!.Value - l.CreatedAt).TotalHours);
//            return new ModerationKpiDto
//            {
//                Date = date,
//                AvgApprovalTimeHours = approvalTimes.Any() ? approvalTimes.Average() : 0,
//                ApprovedCount = dayListings.Count(l => l.ModerationStatus == ListingModerationStatus.Approved),
//                RejectedCount = dayListings.Count(l => l.ModerationStatus == ListingModerationStatus.Rejected),
//                PendingCount = dayListings.Count(l => l.ModerationStatus == ListingModerationStatus.Pending),
//                FlaggedCount = dayListings.Count(l => l.IsFlagged)
//            };
//        }
//    }
//}
using Wanas.Application.DTOs.Analytics;
using Wanas.Application.Interfaces;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IUnitOfWork _uow;

        public AnalyticsService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<AnalyticsSummaryDto> GetSummaryAsync(DateOnly date)
        {
            // CHANGE: Filter listings created ON the selected date
            var listings = await _uow.Listings.FindAsync(l =>
                DateOnly.FromDateTime(l.CreatedAt) == date);

            // CHANGE: Filter logs created ON the selected date
            var logs = await _uow.TrafficLogs.FindAsync(t =>
                DateOnly.FromDateTime(t.CreatedAt) == date);

            var trafficCount = logs.Count();

            var activeUsers = logs
                .Where(t => t.UserId != null)
                .Select(t => t.UserId)
                .Distinct()
                .Count();

            return new AnalyticsSummaryDto
            {
                Date = date,
                TotalListings = listings.Count(),
                PendingListings = listings.Count(l => l.ModerationStatus == ListingModerationStatus.Pending),
                ApprovedListings = listings.Count(l => l.ModerationStatus == ListingModerationStatus.Approved),
                RejectedListings = listings.Count(l => l.ModerationStatus == ListingModerationStatus.Rejected),
                FlaggedListings = listings.Count(l => l.IsFlagged),
                ActiveUsers = activeUsers,
                Requests = trafficCount
            };
        }

        public async Task<IEnumerable<TrafficPointDto>> GetTrafficAsync(DateOnly from, DateOnly to)
        {
            var start = from.ToDateTime(TimeOnly.MinValue);
            var end = to.ToDateTime(TimeOnly.MaxValue);

            var logs = await _uow.TrafficLogs.FindAsync(t =>
                t.CreatedAt >= start && t.CreatedAt <= end);

            return logs.Select(l => new TrafficPointDto
            {
                Timestamp = l.CreatedAt,
                Path = l.Path,
                Method = l.Method,
                Authenticated = l.UserId != null
            });
        }

        public async Task<ModerationKpiDto> GetModerationKpisAsync(DateOnly date)
        {
            var allListings = await _uow.Listings.GetAllAsync();

            // CHANGE: Filter for listings that were moderated (Approved/Rejected) ON this specific date
            var moderatedToday = allListings.Where(l =>
                l.ModeratedAt.HasValue &&
                DateOnly.FromDateTime(l.ModeratedAt.Value) == date);

            // For Pending, we usually want to know how many *new* listings are pending from this date
            var pendingToday = allListings.Where(l =>
                l.ModerationStatus == ListingModerationStatus.Pending &&
                DateOnly.FromDateTime(l.CreatedAt) == date);

            var flaggedToday = allListings.Where(l =>
                l.IsFlagged &&
                DateOnly.FromDateTime(l.CreatedAt) == date); // Or UpdatedAt if available

            var approved = moderatedToday.Where(l => l.ModerationStatus == ListingModerationStatus.Approved);
            var rejected = moderatedToday.Where(l => l.ModerationStatus == ListingModerationStatus.Rejected);

            var approvalTimes = approved
                .Select(l => (l.ModeratedAt!.Value - l.CreatedAt).TotalHours);

            return new ModerationKpiDto
            {
                Date = date,
                AvgApprovalTimeHours = approvalTimes.Any() ? approvalTimes.Average() : 0,
                ApprovedCount = approved.Count(),
                RejectedCount = rejected.Count(),
                PendingCount = pendingToday.Count(),
                FlaggedCount = flaggedToday.Count()
            };
        }
    }
}