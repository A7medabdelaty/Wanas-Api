using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        //Default Data
        builder.HasData([
            new ApplicationRole
            {
                Id = "1bb1f291-7b9c-4071-9db8-7a8240138a44",
                Name = "Admin",
                NormalizedName ="ADMIN",
                ConcurrencyStamp = "94016b54-918e-496a-a280-c20e922839a8"   
            },
            new ApplicationRole
            {
                Id = "cc1e603c-66e6-4f71-bf93-cff53ed896d3",
                Name = "Owner",
                NormalizedName = "OWNER",
                ConcurrencyStamp = "c5df770b-786a-4cd0-aa0f-8b67eb126db9",
                
            },
            new ApplicationRole
            {
                Id = "81ae9adf-5636-4cbb-ac47-96eefcc348c0",
                Name = "Renter",
                NormalizedName = "RENTER",
                ConcurrencyStamp = "76ccf8b9-9885-458b-811a-4bdd707c235e",
                IsDefault = true
            }
        ]);
    }

    
}