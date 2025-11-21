using Microsoft.EntityFrameworkCore;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
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
        public IUserRepository Users { get; }
        public IUserPreferenceRepository UserPreferences { get; }
        public IListingRepository Listings { get; }
        public IAuditLogRepository AuditLogs { get; }
        public IAppealRepository Appeals { get; }


        public IReportRepository Reports { get; }
        public IReportPhotoRepository ReportPhotos { get; }

        public UnitOfWork(AppDBContext context,
                          IChatRepository chats,
                          IMessageRepository messages,
                          IChatParticipantRepository chatParticipants,
                          IReportRepository reports,
                          IReportPhotoRepository reportPhotos,
                          IUserRepository users,
                          IUserPreferenceRepository userPreferences,
                          IListingRepository listings,
                          IAuditLogRepository auditLogs,
                          IAppealRepository appeals)
        {
            _context = context;
            Chats = chats;
            Messages = messages;
            ChatParticipants = chatParticipants;
            Users = users;
            UserPreferences = userPreferences;
            Listings = listings;
            AuditLogs = auditLogs;
            Appeals = appeals;
            Reports = reports;
            ReportPhotos = reportPhotos;
        }

        public async Task<int> CommitAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
