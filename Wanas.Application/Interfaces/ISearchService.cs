using Wanas.Application.DTOs.Search;

namespace Wanas.Application.Interfaces
{
    public interface ISearchService
    {
        Task<ListingSearchResponseDto> SearchListingsAsync(ListingSearchRequestDto request);
    }
}
