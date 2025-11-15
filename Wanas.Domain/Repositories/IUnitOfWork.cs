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
        Task<int> CommitAsync();
    }
}
