namespace Wanas.Domain.Entities
{
    public class DocumentAccessLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DocumentId { get; set; }
        public string AccessedBy { get; set; } = default!; // User ID
        public string Action { get; set; } = default!; // "View", "Download", "Upload", "Delete"
        public string IpAddress { get; set; } = default!;
        public string? UserAgent { get; set; }
        public DateTime AccessedAt { get; set; } = DateTime.UtcNow;
        public string? AdditionalInfo { get; set; }

        // Navigation Properties
        public virtual VerificationDocument Document { get; set; } = default!;
        public virtual ApplicationUser User { get; set; } = default!;
    }
}
