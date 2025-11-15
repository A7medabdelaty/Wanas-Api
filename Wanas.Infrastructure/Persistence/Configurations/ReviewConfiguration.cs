using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.ReviewId);

            builder.Property(r => r.TargetType)
                   .IsRequired();

            builder.Property(r => r.TargetId)
                   .IsRequired();

            builder.Property(r => r.Rating)
                   .IsRequired()
                   .HasColumnType("tinyint");

            builder.Property(r => r.Comment)
                   .HasMaxLength(2000);

            builder.Property(r => r.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(r => r.Reviewer)
                   .WithMany()
                   .HasForeignKey(r => r.ReviewerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.TargetType, r.TargetId });

            builder.Property(r => r.TargetType).HasConversion<string>();
        }
    }
}
