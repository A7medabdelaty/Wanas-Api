using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class ListingConfiguration : IEntityTypeConfiguration<Listing>
    {
        public void Configure(EntityTypeBuilder<Listing> builder)
        {
            builder.HasKey(l => l.Id);
            builder.Property(l => l.Title).HasMaxLength(50);
            builder.Property(l => l.Description).HasMaxLength(350);
            builder.Property(l => l.City).HasMaxLength(50);
            builder.Property(l => l.Address).HasMaxLength(100);
            builder.Property(l => l.MonthlyPrice).HasColumnType("decimal(18,2)");
            builder.Property(l => l.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");
            builder.Property(l => l.IsActive).HasColumnType("bit");
            builder
                .HasIndex(l => l.City)
                .HasDatabaseName("IX_Listing_City"); 

            builder
                .HasOne(l => l.Owner)
                .WithMany(o => o.Listings)
                .HasForeignKey(o => o.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
