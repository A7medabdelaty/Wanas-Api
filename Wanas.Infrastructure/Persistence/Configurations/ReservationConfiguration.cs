using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.ToTable("Reservations");

            builder.HasKey(r => r.Id);

            // Reservation → Listing (many reservations per listing)
            builder.HasOne(r => r.Listing)
                .WithMany(l => l.Reservations)
                .HasForeignKey(r => r.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservation → User
            builder.HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservation → BedReservation (many-to-many)
            builder.HasMany(r => r.Beds)
                .WithOne(br => br.Reservation)
                .HasForeignKey(br => br.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(r => r.TotalPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(r => r.DepositAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(r => r.PaymentStatus)
                .HasConversion<int>();
        }
    }
}
