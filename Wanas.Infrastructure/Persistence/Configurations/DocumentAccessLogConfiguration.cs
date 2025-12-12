using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class DocumentAccessLogConfiguration : IEntityTypeConfiguration<DocumentAccessLog>
    {
        public void Configure(EntityTypeBuilder<DocumentAccessLog> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.AccessedBy)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(d => d.Action)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.IpAddress)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(d => d.UserAgent)
                .HasMaxLength(500);

            builder.Property(d => d.AccessedAt)
                .IsRequired();

            builder.Property(d => d.AdditionalInfo)
                .HasMaxLength(1000);

            // Relationships
            builder.HasOne(d => d.Document)
                .WithMany(v => v.AccessLogs)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.User)
                .WithMany(u => u.DocumentAccessLogs)
                .HasForeignKey(d => d.AccessedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(d => d.DocumentId);
            builder.HasIndex(d => d.AccessedBy);
            builder.HasIndex(d => d.AccessedAt);
        }
    }
}
