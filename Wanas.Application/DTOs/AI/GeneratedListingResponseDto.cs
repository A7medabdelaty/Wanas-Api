
namespace Wanas.Application.DTOs.AI
{
    public class GeneratedListingResponseDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal SuggestionPrice { get; set; }
        public string Location { get; set; } = null!;
        public string RoomType { get; set; } = null!;
        public List<string> Amenities { get; set; } = new();
        public List<string>SuggestedPhotoUrls { get; set; } = new();
        public List<string>Tags { get; set; } = new();
        public bool ReadyToPublish { get; set; }


    }
}
