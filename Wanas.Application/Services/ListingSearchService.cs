using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Wanas.Application.DTOs.Listing;
using Wanas.Application.DTOs.Search;
using Wanas.Application.Interfaces;
using Wanas.Application.QueryBuilders;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class ListingSearchService : IListingSearchService
    {
        private readonly AppDbContext _uow;
        private readonly IMapper _mapper;

        public ListingSearchService(AppDbContext uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<ListingSearchResponseDto> SearchListingsAsync(ListingSearchRequestDto request)
        {
            var repo = _uow.Listings;

            // Start with query from repository (includes already applied)
            var query = repo.GetQueryableWithIncludes();

            // Apply keyword search using repository logic
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                query = repo.ApplyKeywordSearch(query, request.Keyword);
            }

            // Apply filters (Application layer)
            query = ListingSearchQueryBuilder.ApplyFilters(query, request);

            // Apply sorting
            query = ListingSearchQueryBuilder.ApplySorting(query, request.SortBy);

            // Pagination metadata
            var totalCount = await query.CountAsync();
            var totalPages = (int) Math.Ceiling(totalCount / (double) request.PageSize);

            // Fetch page results
            var listings = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ProjectTo<ListingCardDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new ListingSearchResponseDto
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Listings = listings
            };
        }
    
    }
}
