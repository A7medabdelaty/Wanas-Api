using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class BedConfiguration : IEntityTypeConfiguration<Bed>
    {
        public void Configure(EntityTypeBuilder<Bed> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(p => p.IsAvailable).HasColumnType("bit");
            builder.HasIndex(b => b.RenterId);

            builder.HasOne(b => b.Room)
            .WithMany(r => r.Beds)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.Renter)
                .WithMany(u => u.Beds)
                .HasForeignKey(b => b.RenterId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
