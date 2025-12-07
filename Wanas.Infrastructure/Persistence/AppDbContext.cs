using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence
{
    public class AppDBContext(DbContextOptions options) : IdentityDbContext<ApplicationUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDBContext).Assembly);
        }

        public new DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatParticipant> ChatParticipants { get; set; }
        public DbSet<MessageReadReceipt> MessageReadReceipts { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<Listing> Listings { get; set; }
        public DbSet<ListingPhoto> ListingPhotos { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportPhoto> ReportPhotos { get; set; }
        public DbSet<ApartmentListing> ApartmentListings { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Appeal> Appeals { get; set; }
        public DbSet<TrafficLog> TrafficLogs { get; set; }
        public DbSet<DailyMetrics> DailyMetrics { get; set; }
        public DbSet<Commission> Commissions { get; set; }
        public DbSet<Payout> Payouts { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<BookingApproval> BookingApprovals { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}
