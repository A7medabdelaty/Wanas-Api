using Wanas.Domain.Enums;

namespace Wanas.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for Appeal
    /// </summary>
    public class AppealDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public AppealType AppealType { get; set; }
        public string AppealTypeText { get; set; } = string.Empty; // "Ban" or "Suspension"
        public string Reason { get; set; } = string.Empty;
        public AppealStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty; // "Pending", "Approved", "Rejected"
        public string? ReviewedByAdminId { get; set; }
        public string? ReviewedByAdminName { get; set; }
        public string? AdminResponse { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
