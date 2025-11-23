using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public sealed class CommissionConfiguration : IEntityTypeConfiguration<Commission>
    {
        public void Configure(EntityTypeBuilder<Commission> builder)
        {
            builder.Property(c => c.PlatformPercent).HasColumnType("decimal(5,4)");
            builder.Property(c => c.PlatformAmount).HasColumnType("decimal(18,2)");
            builder.HasIndex(c => c.PaymentId);
        }
    }
}