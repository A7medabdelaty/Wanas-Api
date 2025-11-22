namespace Wanas.Application.DTOs.Listing
{
    public class ListingCardDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string City { get; set; }
        public int MonthlyPrice { get; set; }
        public string? MainPhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // Optional but common in listing cards:
        public int? AvailableRooms { get; set; }
        public int? AvailableBeds { get; set; }
        public bool HasInternet { get; set; }
    }

}
