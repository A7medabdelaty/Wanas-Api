namespace Wanas.Application.Settings
{
    public class VerificationSettings
    {
        public const string SectionName = "VerificationSettings";

        public int DocumentRetentionDays { get; set; } = 30; // Keep documents for 30 days after verification
        public int MaxFileSizeInMB { get; set; } = 10; // Max 10MB per file
        public string[] AllowedFileExtensions { get; set; } = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
        public string EncryptionKey { get; set; } = default!; // Will be loaded from environment
        public string EncryptionSalt { get; set; } = default!; // Will be loaded from environment
        public bool EnableAutoDelete { get; set; } = true;
    }
}
