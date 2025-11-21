namespace Wanas.Domain.Repositories
{
    public interface AppDbContext : IDisposable
    {
        IChatRepository Chats { get; }
        IMessageRepository Messages { get; }
        IChatParticipantRepository ChatParticipants { get; }
        IUserRepository Users { get; }
        IUserPreferenceRepository UserPreferences { get; }
        IListingRepository Listings { get; }
        IAuditLogRepository AuditLogs { get; }
        IAppealRepository Appeals { get; }
        Task<int> CommitAsync();
    }
}
