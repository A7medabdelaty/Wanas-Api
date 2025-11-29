namespace Wanas.Application.DTOs.Search
{
    public class ListingSearchRequestDto
    {
        public string? Keyword { get; set; }
        public string? City { get; set; }

        // Price
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }

        // Rooms & beds
        public int? MinRooms { get; set; }
        public int? MaxRooms { get; set; }
        public int? MinBeds { get; set; }
        public int? MaxBeds { get; set; }

        // Area
        public int? MinArea { get; set; }
        public int? MaxArea { get; set; }

        // Floor
        public int? MinFloor { get; set; }
        public int? MaxFloor { get; set; }

        // Availability
        public bool? OnlyAvailable { get; set; }

        // Features
        public bool? HasInternet { get; set; }
        public bool? HasKitchen { get; set; }
        public bool? HasElevator { get; set; }
        public bool? HasAirConditioner { get; set; }
        public bool? HasFans { get; set; }
        public bool? HasPrivateBathroom { get; set; }
        public bool? IsPetFriendly { get; set; }
        public bool? IsSmokingAllowed { get; set; }

        // Sorting & paging
        public string? SortBy { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
