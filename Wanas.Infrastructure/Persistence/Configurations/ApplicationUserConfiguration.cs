using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(au => au.FullName).HasMaxLength(150);
            builder.Property(au => au.City).HasMaxLength(75);
            builder.Property(au => au.Bio).HasMaxLength(1000);
            builder.Property(au => au.PhoneNumber).HasMaxLength(20);

            builder.Property(au => au.ProfileType).HasConversion<string>();
        }
    }
}
