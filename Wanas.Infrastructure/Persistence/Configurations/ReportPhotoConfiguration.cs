using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations;

internal class ReportPhotoConfiguration : IEntityTypeConfiguration<ReportPhoto>
{
    public void Configure(EntityTypeBuilder<ReportPhoto> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.URL).HasMaxLength(400);
        builder
            .HasOne(r => r.Report)
            .WithMany(r => r.ReportPhotos)
            .HasForeignKey(x => x.ReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
