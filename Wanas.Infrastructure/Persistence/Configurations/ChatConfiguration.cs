using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .HasMaxLength(100);

            builder.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Private chats have ListingId → this relationship handled from Listing side
            // Group chat has no ListingId at all

            // Participants
            builder.HasMany(c => c.ChatParticipants)
                .WithOne(cp => cp.Chat)
                .HasForeignKey(cp => cp.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            // Messages
            builder.HasMany(c => c.Messages)
                .WithOne(m => m.Chat)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Listing)
            .WithMany(l => l.Chats)
            .HasForeignKey(c => c.ListingId)
            .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
