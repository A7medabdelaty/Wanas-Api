using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.PricePerBed).HasColumnType("decimal(18,2)");
            builder.Property(r => r.IsAvailable).HasColumnType("bit");

            builder.HasOne(r => r.ApartmentListing)
                .WithMany(a => a.Rooms)
                .HasForeignKey(r => r.ApartmentListingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Beds)
                .WithOne(b => b.Room)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}
