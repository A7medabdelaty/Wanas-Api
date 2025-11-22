namespace Wanas.Application.DTOs.Search
{
    public class ListingSearchRequestDto
    {
        public string? City { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public bool? OnlyAvailable { get; set; }
        public string? Keyword { get; set; }

        // Optional advanced filters
        public bool? HasInternet { get; set; }
        public bool? HasKitchen { get; set; }
        public bool? HasElevator { get; set; }
        public List<string>? Preferences { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;

        // Sorting (price, newest, relevance)
        public string? SortBy { get; set; }
    }
}
