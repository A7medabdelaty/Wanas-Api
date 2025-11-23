using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class DailyMetricsConfiguration : IEntityTypeConfiguration<DailyMetrics>
    {
        public void Configure(EntityTypeBuilder<DailyMetrics> builder)
        {
            builder.ToTable("DailyMetrics");

            builder.Property(x => x.Date).HasColumnType("date");
            builder.Property(x => x.LastCalculatedAt).HasColumnType("datetime2");

            builder.HasIndex(x => x.Requests);
            builder.HasIndex(x => x.ActiveUsers);
            
            builder.Property(x => x.TotalListings).HasDefaultValue(0);
            builder.Property(x => x.PendingListings).HasDefaultValue(0);
            builder.Property(x => x.ApprovedListings).HasDefaultValue(0);
            builder.Property(x => x.RejectedListings).HasDefaultValue(0);
            builder.Property(x => x.FlaggedListings).HasDefaultValue(0);
            builder.Property(x => x.TotalUsers).HasDefaultValue(0);
            builder.Property(x => x.ActiveUsers).HasDefaultValue(0);
            builder.Property(x => x.Requests).HasDefaultValue(0);
        }
    }
}
