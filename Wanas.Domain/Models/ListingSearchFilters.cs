namespace Wanas.Domain.Models
{
    public class ListingSearchFilters
    {
        public string City { get; set; }

        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }

        public bool? AvailableRoomsOnly { get; set; }
        public bool? AvailableBedsOnly { get; set; }

        public bool? HasElevator { get; set; }
        public bool? HasKitchen { get; set; }
        public bool? HasInternet { get; set; }

        public bool? IsPetFriendly { get; set; }
        public bool? IsSmokingAllowed { get; set; }

        public int? MinArea { get; set; }
        public int? MaxArea { get; set; }

        public int? MinRooms { get; set; }
        public int? MaxRooms { get; set; }

        public int? MinBeds { get; set; }
        public int? MaxBeds { get; set; }

        public string Keyword { get; set; } // Full Text Search

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting
        public bool SortByRelevance { get; set; } = true;
    }
}
