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
        public IReviewRepository Reviews { get; }
        //public IListingRepository Listings { get; }
        //public IListingPhotoRepository ListingPhotos { get; }
        //public ICommentRepository Comments { get; }

        public UnitOfWork(AppDBContext context,
                          IChatRepository chats,
                          IMessageRepository messages,
                          IChatParticipantRepository chatParticipants,
                          IReviewRepository reviews)
        {
            _context = context;
            Chats = chats;
            Messages = messages;
            ChatParticipants = chatParticipants;
            Reviews = reviews;
            //Listings = listings;    
            //ListingPhotos = listingPhotos;
            //Comments = comments;
        }

        public async Task<int> CommitAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
