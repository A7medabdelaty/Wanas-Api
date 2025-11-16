
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;

namespace Wanas.Infrastructure.Persistence.Configurations;
public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder
            .OwnsMany(x => x.RefreshTokens)
            .ToTable("RefreshTokens")
            .WithOwner()
            .HasForeignKey("UserId");

        builder.Property(x => x.FullName).HasMaxLength(200);

        var passwordHasher = new PasswordHasher<ApplicationUser>();

        var admin = new ApplicationUser
        {
            Id = "808cfe62-dd5b-4c25-837d-3df87add03cb",
            FullName = "Wanas Admin",
            UserName = "admin@wanas.com",
            NormalizedUserName = "ADMIN@WANAS.COM",
            Email = "admin@wanas.com",
            NormalizedEmail = "ADMIN@WANAS.COM",
            SecurityStamp = "71ac7750-1375-4d94-9b71-cfd70509373f",
            ConcurrencyStamp = "d7b60902-76de-41cd-b394-4bcb7ad058e3",
            EmailConfirmed = true,
            PhoneNumber= "01234567890",
            CreatedAt = new DateTime(2025, 11, 15, 0, 0, 0, DateTimeKind.Utc),
            Age = 30,
            City = "Cairo",
            Bio = "System Administrator",
            ProfileType =ProfileType.Admin,
            Photo = "",
            IsDeleted = false,
            PasswordHash= "AQAAAAIAAYagAAAAELa2X0xFqZjvpTNRlgpsUPiLoNS8BFTI6VQSb4aUwPk2Wk3UMJ90QYH832zEDknnkA=="
        };

        //admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin@12345");

        builder.HasData(admin);

    }
}
