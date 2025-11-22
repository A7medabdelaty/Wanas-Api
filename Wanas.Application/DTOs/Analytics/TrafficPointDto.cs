namespace Wanas.Application.DTOs.Analytics
{
    public class TrafficPointDto
    {
        public DateTime Timestamp { get; set; }
        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public bool Authenticated { get; set; }
    }
}