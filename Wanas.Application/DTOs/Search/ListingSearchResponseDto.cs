using Wanas.Application.DTOs.Listing;

namespace Wanas.Application.DTOs.Search
{
    public class ListingSearchResponseDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public List<ListingCardDto> Listings { get; set; } = new();
    }
}
