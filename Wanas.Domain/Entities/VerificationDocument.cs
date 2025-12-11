using Wanas.Domain.Enums;

namespace Wanas.Domain.Entities
{
    public class VerificationDocument
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = default!;
        public DocumentType DocumentType { get; set; }
        public string EncryptedFilePath { get; set; } = default!; // Encrypted path to the file
        public string FileHash { get; set; } = default!; // SHA256 hash for integrity verification
        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; } // Admin ID who reviewed
        public string? RejectionReason { get; set; }
        public DateTime? ScheduledDeletionDate { get; set; } // Auto-delete after approval/rejection
        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = default!;
        public virtual ApplicationUser? Reviewer { get; set; }
        public virtual ICollection<DocumentAccessLog> AccessLogs { get; set; } = new List<DocumentAccessLog>();
    }
}
