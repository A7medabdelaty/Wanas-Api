using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class BedReservationConfiguration : IEntityTypeConfiguration<BedReservation>
    {
        public void Configure(EntityTypeBuilder<BedReservation> builder)
        {
            builder.ToTable("BedReservations");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.ListingId).IsRequired();
            builder.Property(x => x.ReservedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasMany(x => x.Items)
                .WithOne(i => i.BedReservation)
                .HasForeignKey(i => i.BedReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class BedReservationItemConfiguration : IEntityTypeConfiguration<BedReservationItem>
    {
        public void Configure(EntityTypeBuilder<BedReservationItem> builder)
        {
            builder.ToTable("BedReservationItems");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.BedId, x.BedReservationId });

            builder.HasOne(x => x.Bed)
                .WithMany()
                .HasForeignKey(x => x.BedId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
