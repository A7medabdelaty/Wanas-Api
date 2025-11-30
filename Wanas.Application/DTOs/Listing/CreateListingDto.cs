using Microsoft.AspNetCore.Http;

namespace Wanas.Application.DTOs.Listing
{
    public class CreateListingDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public int MonthlyPrice { get; set; }
        public bool HasElevator { get; set; }
        public int Floor { get; set; }
        public int AreaInSqMeters { get; set; }
        public int TotalBathrooms { get; set; }
        public bool HasKitchen { get; set; }
        public bool HasInternet { get; set; }
        public bool HasAirConditioner { get; set; }
        public bool HasFans { get; set; }
        public bool IsPetFriendly { get; set; }
        public bool IsSmokingAllowed { get; set; }
        public List<CreateRoomDto> Rooms { get; set; }
        public List<IFormFile> Photos { get; set; }
    }

}
