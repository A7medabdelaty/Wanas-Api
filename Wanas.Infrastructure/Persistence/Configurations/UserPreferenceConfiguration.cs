 using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
    {
        public void Configure(EntityTypeBuilder<UserPreference> builder)
        {
            builder.HasKey(p => p.Id);

            builder.HasOne(p => p.User)
               .WithOne(u => u.UserPreference)
               .HasForeignKey<UserPreference>(p => p.UserId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.Property(p => p.City).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Job).HasMaxLength(100);
            builder.Property(p => p.University).HasMaxLength(100);
            builder.Property(p => p.Major).HasMaxLength(100);

            // for enum properties, store as strings not as integers
            builder.Property(p => p.Gender).HasConversion<string>();
            builder.Property(p => p.Children).HasConversion<string>();
            builder.Property(p => p.Visits).HasConversion<string>();
            builder.Property(p => p.Smoking).HasConversion<string>();
            builder.Property(p => p.Pets).HasConversion<string>();
            builder.Property(p => p.SleepSchedule).HasConversion<string>();
            builder.Property(p => p.SocialLevel).HasConversion<string>();
            builder.Property(p => p.NoiseToleranceLevel).HasConversion<string>();

            builder.HasIndex(p => p.City);
        }

    }
}
