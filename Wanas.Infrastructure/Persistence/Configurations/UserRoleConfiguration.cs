using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wanas.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {
        //Default Data
        builder.HasData(new IdentityUserRole<string>
        {
            UserId = "808cfe62-dd5b-4c25-837d-3df87add03cb",
            RoleId = "1bb1f291-7b9c-4071-9db8-7a8240138a44" 
        });
    }

}