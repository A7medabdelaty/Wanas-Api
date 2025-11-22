using Wanas.Domain.Repositories.Listings;

namespace Wanas.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> CommitAsync();
        IChatRepository Chats { get; }
        IMessageRepository Messages { get; }
        IChatParticipantRepository ChatParticipants { get; }
        IUserRepository Users { get; }
        IUserPreferenceRepository UserPreferences { get; }
        IAuditLogRepository AuditLogs { get; }
        IAppealRepository Appeals { get; }

        IReportPhotoRepository ReportPhotos { get; }
        IReportRepository Reports { get; }

        IReviewRepository Reviews { get; }
        IListingRepository Listings { get; }
        IListingPhotoRepository ListingPhotos { get; }
        ICommentRepository Comments { get; }
    }
}
