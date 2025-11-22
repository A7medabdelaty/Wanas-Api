namespace Wanas.Domain.Entities
{
    public class TrafficLog
    {
        public long Id { get; set; }
        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}