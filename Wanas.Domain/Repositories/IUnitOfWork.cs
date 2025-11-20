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

        IReportPhotoRepository ReportPhotos { get; }
        IReportRepository Reports { get; }
        Task<int> CommitAsync();
    }
}
