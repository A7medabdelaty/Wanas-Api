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
        public string Floor { get; set; }
        public int AreaInSqMeters { get; set; }

        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int TotalBeds { get; set; }
        public int AvailableBeds { get; set; }
        public int TotalBathrooms { get; set; }

        public bool HasKitchen { get; set; }
        public bool HasInternet { get; set; }

        public List<IFormFile> Photos { get; set; }
    }

}
