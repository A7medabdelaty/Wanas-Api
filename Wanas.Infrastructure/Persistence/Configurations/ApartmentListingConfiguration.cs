using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class ApartmentListingConfiguration : IEntityTypeConfiguration<ApartmentListing>
    {
        public void Configure(EntityTypeBuilder<ApartmentListing> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Address).HasMaxLength(200);
            builder.Property(a => a.Floor).HasMaxLength(50);

            builder.HasOne(a => a.Listing)
                .WithOne(l => l.ApartmentListing)
                .HasForeignKey<ApartmentListing>(a => a.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
