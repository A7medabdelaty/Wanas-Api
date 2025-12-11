using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class VerificationDocumentConfiguration : IEntityTypeConfiguration<VerificationDocument>
    {
        public void Configure(EntityTypeBuilder<VerificationDocument> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(v => v.DocumentType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(v => v.EncryptedFilePath)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(v => v.FileHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(v => v.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(VerificationStatus.Pending);

            builder.Property(v => v.UploadedAt)
                .IsRequired();

            builder.Property(v => v.ReviewedBy)
                .HasMaxLength(450);

            builder.Property(v => v.RejectionReason)
               .HasMaxLength(1000);

            builder.Property(v => v.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(v => v.User)
                .WithMany(u => u.VerificationDocuments)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.Reviewer)
                .WithMany(u => u.ReviewedVerificationDocuments)
                .HasForeignKey(v => v.ReviewedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(v => v.AccessLogs)
                .WithOne(a => a.Document)
                .HasForeignKey(a => a.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(v => v.UserId);
            builder.HasIndex(v => v.Status);
            builder.HasIndex(v => new { v.UserId, v.DocumentType });
            builder.HasIndex(v => v.ScheduledDeletionDate);
        }
    }
}
