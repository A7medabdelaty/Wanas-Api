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

            // Composite Key
            builder.HasKey(br => new { br.BedId, br.ReservationId });

            builder.HasOne(br => br.Bed)
                .WithMany(b => b.BedReservations)
                .HasForeignKey(br => br.BedId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(br => br.Reservation)
                .WithMany(r => r.Beds)
                .HasForeignKey(br => br.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
