using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class MessageReadReceiptConfiguration : IEntityTypeConfiguration<MessageReadReceipt>
    {
        public void Configure(EntityTypeBuilder<MessageReadReceipt> builder)
        {
            builder.HasKey(mrr => mrr.Id);

            builder.HasOne(mrr => mrr.Message)
                .WithMany(m => m.ReadReceipts)
                .HasForeignKey(mrr => mrr.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(mrr => mrr.User)
                .WithMany(u => u.MessageReadReceipts)
                .HasForeignKey(mrr => mrr.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.HasIndex(rr => new { rr.MessageId, rr.UserId }).IsUnique();
        }
    }
}
