using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.PaymentId);

            builder.Property(p => p.Amount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(p => p.Status)
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(p => p.PaymentDate)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.TransactionId)
                   .HasMaxLength(200);

            builder.HasOne(p => p.User)
                   .WithMany()
                   .HasForeignKey(p => p.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            //builder.HasOne(p => p.Listing)
            //       .WithMany()
            //       .HasForeignKey(p => p.ListingId)
            //       .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
