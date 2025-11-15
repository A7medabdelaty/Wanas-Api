namespace Wanas.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IChatRepository Chats { get; }
        IMessageRepository Messages { get; }

        IReportPhotoRepository ReportPhotos { get; }
        IReportRepository Reports { get; }
        Task<int> CommitAsync();
        IChatParticipantRepository ChatParticipants { get; }
    }
}
