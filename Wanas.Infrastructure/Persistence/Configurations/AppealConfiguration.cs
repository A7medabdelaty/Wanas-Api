using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    internal class AppealConfiguration : IEntityTypeConfiguration<Appeal>
    {
        public void Configure(EntityTypeBuilder<Appeal> builder)
        {
            // Primary Key
            builder.HasKey(a => a.Id);

            // Properties
            builder.Property(a => a.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(a => a.AppealType)
                .IsRequired();

            builder.Property(a => a.Reason)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(a => a.Status)
                .IsRequired();

            builder.Property(a => a.ReviewedByAdminId)
                .HasMaxLength(450);

            builder.Property(a => a.AdminResponse)
                .HasMaxLength(1000);

            builder.Property(a => a.CreatedAt)
                .IsRequired();

            // Relationships

            // Appeal -> User (who submitted the appeal)
            builder.HasOne(a => a.User)
                .WithMany(u => u.Appeals)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

            // Appeal -> ReviewedByAdmin (admin who reviewed)
            builder.HasOne(a => a.ReviewedByAdmin)
                .WithMany(u => u.ReviewedAppeals)
                .HasForeignKey(a => a.ReviewedByAdminId)
                .OnDelete(DeleteBehavior.Restrict) // Prevent cascading delete
                .IsRequired(false); // Nullable - not all appeals are reviewed

            // Indexes for performance
            builder.HasIndex(a => a.UserId)
                .HasDatabaseName("IX_Appeals_UserId");

            builder.HasIndex(a => a.Status)
                .HasDatabaseName("IX_Appeals_Status");

            builder.HasIndex(a => a.CreatedAt)
                .HasDatabaseName("IX_Appeals_CreatedAt");

            builder.HasIndex(a => new { a.UserId, a.Status })
                .HasDatabaseName("IX_Appeals_UserId_Status");

            // Table name
            builder.ToTable("Appeals");
        }
    }
}
