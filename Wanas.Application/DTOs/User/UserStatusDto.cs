namespace Wanas.Application.DTOs.User
{
    public class UserStatusDto
    {
        public bool IsBanned { get; set; }
        public string? BanReason { get; set; }
        public DateTime? BannedAt { get; set; }

        public bool IsSuspended { get; set; }
        public DateTime? SuspendedUntil { get; set; }
        public string? SuspensionReason { get; set; }
        public DateTime? SuspendedAt { get; set; }

        public bool IsActive => !IsBanned && (!IsSuspended || (SuspendedUntil.HasValue && SuspendedUntil.Value < DateTime.UtcNow));
    }
}
