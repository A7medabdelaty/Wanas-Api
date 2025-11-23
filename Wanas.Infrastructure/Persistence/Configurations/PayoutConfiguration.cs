using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public sealed class PayoutConfiguration : IEntityTypeConfiguration<Payout>
    {
        public void Configure(EntityTypeBuilder<Payout> builder)
        {
            builder.Property(p => p.GrossAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.CommissionTotal).HasColumnType("decimal(18,2)");
            builder.Property(p => p.NetAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.Currency).HasMaxLength(10);
            builder.Property(p => p.Status).HasMaxLength(30);
            builder.HasIndex(p => new { p.HostUserId, p.Status });
            builder.HasIndex(p => p.PeriodStart);
            builder.HasIndex(p => p.PeriodEnd);
            builder.HasOne(p => p.HostUser)
                   .WithMany(u => u.Payouts)
                   .HasForeignKey(p => p.HostUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}