using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class MatchConfiguration : IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> builder)
        {
            builder.HasKey(m => m.MatchId);

            builder.Property(m => m.MatchScore)
                   .IsRequired();

            builder.Property(m => m.MatchedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(m => m.Status)
                   .IsRequired();

            // User (one user can have many matches)
            builder.HasOne(m => m.User)
                   .WithMany(u => u.Matches)
                   .HasForeignKey(m => m.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Listing)        // Match.Listing navigation
                .WithMany(l => l.Matches)      // Listing.Matches collection
                .HasForeignKey(m => m.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(m => m.Status).HasConversion<string>();
        }
    }
}
