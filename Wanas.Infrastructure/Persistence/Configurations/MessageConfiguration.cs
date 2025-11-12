using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(m => m.Id);

            builder.HasOne(m => m.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(m => m.ReadReceipts)
                .WithOne(rr => rr.Message)
                .HasForeignKey(rr => rr.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(m => m.TextContent).HasMaxLength(2000);
            builder.Property(m => m.MediaUrl).HasMaxLength(500);
            builder.Property(m => m.MediaMimeType).HasMaxLength(100);

            builder.Property(m => m.MessageType).HasConversion<string>();

            builder.HasIndex(m => m.ChatId);
            builder.HasIndex(m => m.SenderId);
            builder.HasIndex(m => m.SentAt);
        }
    }
}
