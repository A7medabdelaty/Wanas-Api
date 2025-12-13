using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs.Listing
{
    public class TenantDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string? Photo { get; set; }
        public Gender Gender { get; set; }
        public int? Age { get; set; }
        public string? Bio { get; set; }
    }
}
