
namespace Wanas.Application.DTOs.Listing
{
    public class ListingDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public string City { get; set; }
        public string Address { get; set; }

        public int MonthlyPrice { get; set; }

        public bool HasElevator { get; set; }
        public int Floor { get; set; }
        public int AreaInSqMeters { get; set; }

        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int TotalBeds { get; set; }
        public int AvailableBeds { get; set; }
        public int TotalBathrooms { get; set; }

        public bool HasKitchen { get; set; }
        public bool HasInternet { get; set; }
        public bool HasAirConditioner { get; set; }
        public bool HasFans { get; set; }

        public bool IsPetFriendly { get; set; }
        public bool IsSmokingAllowed { get; set; }

        public List<ListingPhotoDto> ListingPhotos { get; set; }
        public List<CommentDto> Comments { get; set; }
    }

}
