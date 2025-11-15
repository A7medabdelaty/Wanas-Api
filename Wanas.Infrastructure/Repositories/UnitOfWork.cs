using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDBContext _context;

        public IChatRepository Chats { get; }
        public IMessageRepository Messages { get; }
        public IChatParticipantRepository ChatParticipants { get; }


        public IReportRepository Reports { get; }
        public IReportPhotoRepository ReportPhotos { get; }

        public UnitOfWork(AppDBContext context,
                          IChatRepository chats,
                          IMessageRepository messages,
                          IChatParticipantRepository chatParticipants,
                          IReportRepository reports,
                          IReportPhotoRepository reportPhotos)
        {
            _context = context;
            Chats = chats;
            Messages = messages;
            ChatParticipants = chatParticipants;
            Reports = reports;
            ReportPhotos = reportPhotos;
        }

        public async Task<int> CommitAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
