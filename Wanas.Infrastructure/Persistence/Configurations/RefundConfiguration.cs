using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
    {
        public void Configure(EntityTypeBuilder<Refund> builder)
        {
            builder.Property(r => r.Amount).HasColumnType("decimal(18,2)");
            builder.Property(r => r.Status).HasMaxLength(30);
            builder.Property(r => r.Reason).HasMaxLength(300);
            builder.HasIndex(r => r.PaymentId);
        }
    }
}