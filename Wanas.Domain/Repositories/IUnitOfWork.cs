using Wanas.Domain.Repositories.Listings;

namespace Wanas.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> CommitAsync();
        IChatRepository Chats { get; }
        IMessageRepository Messages { get; }
        IChatParticipantRepository ChatParticipants { get; }
        IReviewRepository Reviews { get; }
        //IListingRepository Listings { get; }
        //IListingPhotoRepository ListingPhotos { get; }
        //ICommentRepository Comments { get; }
    }
}
