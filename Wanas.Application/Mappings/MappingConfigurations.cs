using Mapster;
using Wanas.Application.DTOs.Authentication;
using Wanas.Domain.Entities;

namespace Wanas.Application.Mappings;

public class MappingConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        
        config.NewConfig<RegisterRequest, ApplicationUser>()
            .Map(dest => dest.UserName, src => src.Email);
    }
}
