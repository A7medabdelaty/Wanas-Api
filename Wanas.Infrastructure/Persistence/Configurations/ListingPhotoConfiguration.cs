using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class ListingPhotoConfiguration : IEntityTypeConfiguration<ListingPhoto>
    {
        public void Configure(EntityTypeBuilder<ListingPhoto> builder)
        {
            builder.HasKey(lp => lp.Id);
            builder.Property(lp => lp.URL).HasMaxLength(400);
            builder
                .HasOne(l => l.Listing)
                .WithMany(lp => lp.ListingPhotos)
                .HasForeignKey(lp => lp.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
