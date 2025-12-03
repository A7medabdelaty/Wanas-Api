
namespace Wanas.Application.DTOs.Listing;

    public class ListingPendingDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string City { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ListingPhotoDto> ListingPhotos { get; set; }
    }

