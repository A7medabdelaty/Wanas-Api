namespace Wanas.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IChatRepository Chats { get; }
        IMessageRepository Messages { get; }
        IChatParticipantRepository ChatParticipants { get; }
        IUserRepository Users { get; }
        IUserPreferenceRepository UserPreferences { get; }
        IListingRepository Listings { get; }
        IAuditLogRepository AuditLogs { get; }
        IAppealRepository Appeals { get; }
        IReportPhotoRepository ReportPhotos { get; }
        IReportRepository Reports { get; }
        ITrafficLogRepository TrafficLogs { get; }
        IDailyMetricsRepository DailyMetrics { get; }
        // Revenue
        IPaymentRepository Payments { get; }
        ICommissionRepository Commissions { get; }
        IPayoutRepository Payouts { get; }
        IRefundRepository Refunds { get; }
        Task<int> CommitAsync();
    }
}
