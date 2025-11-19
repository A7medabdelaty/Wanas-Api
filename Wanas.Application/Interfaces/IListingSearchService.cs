using Wanas.Application.DTOs.Search;

namespace Wanas.Application.Interfaces
{
    public interface IListingSearchService
    {
        Task<ListingSearchResponseDto> SearchListingsAsync(ListingSearchRequestDto request);

    }
}
