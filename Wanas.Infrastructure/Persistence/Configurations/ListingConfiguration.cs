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
            builder.Property(l => l.Description).HasMaxLength(2000);
            builder.Property(l => l.City).HasMaxLength(50);
            builder.Property(l => l.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");
            builder.Property(l => l.IsActive).HasColumnType("bit");
            builder
                .HasIndex(l => l.City)
                .HasDatabaseName("IX_Listing_City");

            builder
                .HasOne(l => l.User)
                .WithMany(o => o.Listings)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(l => l.ApartmentListing)
                .WithOne(al => al.Listing)
                .HasForeignKey<ApartmentListing>(al => al.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.ListingPhotos)
                .WithOne(lp => lp.Listing)
                .HasForeignKey(lp => lp.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.Payments)
                .WithOne(p => p.Listing)
                .HasForeignKey(p => p.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.Comments)
                .WithOne(c => c.Listing)
                .HasForeignKey(c => c.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
