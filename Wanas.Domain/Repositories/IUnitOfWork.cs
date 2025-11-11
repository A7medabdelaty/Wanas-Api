namespace Wanas.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IChatRepository Chats { get; }
        IMessageRepository Messages { get; }
        Task<int> CommitAsync();
    }
}
