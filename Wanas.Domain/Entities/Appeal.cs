using System.ComponentModel.DataAnnotations;
using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    /// <summary>
    /// Represents a user appeal against a ban or suspension
    /// </summary>
    public class Appeal
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty;
        public AppealType AppealType { get; set; }

        public string Reason { get; set; } = string.Empty;

        public AppealStatus Status { get; set; } = AppealStatus.Pending;

        public string? ReviewedByAdminId { get; set; }

        public string? AdminResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ApplicationUser? ReviewedByAdmin { get; set; }
    }
}
