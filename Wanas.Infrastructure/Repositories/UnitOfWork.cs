using Microsoft.EntityFrameworkCore;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Domain.Repositories.Listings;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDBContext _context;

        public IChatRepository Chats { get; }
        public IMessageRepository Messages { get; }
        public IChatParticipantRepository ChatParticipants { get; }
        public IUserRepository Users { get; }
        public IUserPreferenceRepository UserPreferences { get; }
        public IAuditLogRepository AuditLogs { get; }
        public IAppealRepository Appeals { get; }
        public IReviewRepository Reviews { get; }
        public IListingRepository Listings { get; }
        public IReportRepository Reports { get; }
        public IReportPhotoRepository ReportPhotos { get; }
        public ITrafficLogRepository TrafficLogs { get; }
        public IDailyMetricsRepository DailyMetrics { get; }
        public IPaymentRepository Payments { get; }
        public ICommissionRepository Commissions { get; }
        public IPayoutRepository Payouts { get; }
        public IRefundRepository Refunds { get; }
        public IListingPhotoRepository ListingPhotos { get; }
        public ICommentRepository Comments { get; }
        public IRoomRepository Rooms { get; }
        public IBookingApprovalRepository BookingApprovals { get; }


        public UnitOfWork(AppDBContext context,
                          IChatRepository chats,
                          IMessageRepository messages,
                          IChatParticipantRepository chatParticipants,
                          IReportRepository reports,
                          IReportPhotoRepository reportPhotos,
                          IUserRepository users,
                          IUserPreferenceRepository userPreferences,
                          IAuditLogRepository auditLogs,
                          IAppealRepository appeals,
                          IReviewRepository reviews,
                          IListingRepository listings,
                          IListingPhotoRepository listingPhotos,
                          ICommentRepository comments,
                          ITrafficLogRepository trafficLogs,
                          IDailyMetricsRepository dailyMetrics,
                          IPaymentRepository payments,
                          ICommissionRepository commissions,
                          IPayoutRepository payouts,
                          IRefundRepository refunds,
                          IRoomRepository rooms,
                          IBookingApprovalRepository bookingApprovals)
        {
            _context = context;
            Chats = chats;
            Messages = messages;
            ChatParticipants = chatParticipants;
            Users = users;
            UserPreferences = userPreferences;
            Listings = listings;
            AuditLogs = auditLogs;
            Appeals = appeals;
            Reports = reports;
            ReportPhotos = reportPhotos;
            TrafficLogs = trafficLogs;
            DailyMetrics = dailyMetrics;
            Payments = payments;
            Commissions = commissions;
            Payouts = payouts;
            Refunds = refunds;
            Reviews = reviews;
            Listings = listings;
            ListingPhotos = listingPhotos;
            Comments = comments;
            Listings = listings;
            ListingPhotos = listingPhotos;
            Rooms = rooms;
            BookingApprovals = bookingApprovals;
        }

        public async Task<int> CommitAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
