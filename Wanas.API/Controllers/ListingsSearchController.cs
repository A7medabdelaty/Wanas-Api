using Microsoft.AspNetCore.Mvc;
using Wanas.Application.DTOs.Search;
using Wanas.Application.Interfaces;

namespace Wanas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingsSearchController : ControllerBase
    {
        private readonly IListingSearchService _searchService;

        public ListingsSearchController(IListingSearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchListings([FromQuery] ListingSearchRequestDto request)
        {
            if (request.Page <= 0)
                request.Page = 1;

            if (request.PageSize <= 0)
                request.PageSize = 12;

            var result = await _searchService.SearchListingsAsync(request);
            return Ok(result);
        }
    }
}
