
namespace Wanas.Application.DTOs.AI
{
    public class GenerateListingRequestDto
    {
        public string? Prompt { get; set; }
        public string? Location { get; set; }
        public string? RoomType { get; set; } // e.g. "private room", "shared room"
        public int? Bedrooms { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public List<string>? Amenities { get; set; } // wifi, ac, heating, pet-friendly...
        public string? TargetAudience { get; set; } // e.g. "students, professionals, female-only","families", "business travelers"
        public List<string>? PropertyRules { get; set; } // e.g. "no smoking", "no parties"
        public List<string>? PhotoUrls { get; set; }
        public bool DraftOnly { get; set; } = true;       // don't publish immediately

    }
}
