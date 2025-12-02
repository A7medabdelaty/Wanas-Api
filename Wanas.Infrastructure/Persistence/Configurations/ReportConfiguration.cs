using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.HasKey(r => r.ReportId);

            builder.Property(r => r.TargetType)
                   .IsRequired();

            builder.Property(r => r.TargetId)
                   .IsRequired();

            builder.Property(r => r.Reason)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(r => r.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(r => r.Reporter)
                   .WithMany(u => u.Reports)
                   .HasForeignKey(r => r.ReporterId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(r => r.Category)
                   .HasConversion<int>();

            builder.HasIndex(r => new { r.TargetType, r.TargetId });

            builder.Property(r => r.TargetType).HasConversion<string>();
            builder.Property(r => r.Status).HasConversion<string>();
        }
    }
}
