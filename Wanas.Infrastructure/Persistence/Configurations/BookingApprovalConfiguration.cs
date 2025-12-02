using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class BookingApprovalConfiguration : IEntityTypeConfiguration<BookingApproval>
    {
        public void Configure(EntityTypeBuilder<BookingApproval> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ListingId)
                .IsRequired();

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.OwnerId)
                .IsRequired();

            builder.Property(x => x.ChatId)
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.ApprovedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // BookingApproval → Listing
            builder.HasOne(x => x.Listing)
                .WithMany(l => l.BookingApprovals)
                .HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.NoAction);

            // BookingApproval → User (Requester)
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // BookingApproval → Owner
            builder.HasOne(x => x.Owner)
                .WithMany()
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            // BookingApproval → Chat
            builder.HasOne(x => x.Chat)
                .WithMany()
                .HasForeignKey(x => x.ChatId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => new { x.ListingId, x.UserId, x.Type })
                .IsUnique();
        }
    }
}
