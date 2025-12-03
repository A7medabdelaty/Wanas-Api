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
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ListingSearchService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<ListingSearchResponseDto> SearchListingsAsync(ListingSearchRequestDto request)
        {
            var repo = _uow.Listings;

            // Start query (includes images, owner, etc.)
            var query = repo.GetQueryableWithIncludes();

            // Apply keyword search (repository layer)
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                query = repo.ApplyKeywordSearch(query, request.Keyword);
            }

            // Apply all filters (price, rooms, beds, area, features, etc.)
            query = ListingSearchQueryBuilder.ApplyFilters(query, request);

            // Apply sorting (price, newest, area, etc.)
            query = ListingSearchQueryBuilder.ApplySorting(query, request.SortBy);

            // Pagination metadata
            var totalCount = await query.CountAsync();
            var totalPages = (int) Math.Ceiling(totalCount / (double) request.PageSize);

            // Get paged records
            var listings = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Where(l=>l.ModerationStatus==Domain.Enums.ListingModerationStatus.Approved)
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
