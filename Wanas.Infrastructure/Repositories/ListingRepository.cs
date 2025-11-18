using Microsoft.EntityFrameworkCore;
using Wanas.Domain.Entities;
using Wanas.Domain.Models;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class ListingRepository : GenericRepository<Listing>, IListingRepository
    {
        public ListingRepository(AppDBContext context) : base(context)
        {
        }

        public async Task<List<Listing>> GetActiveListingsAsync()
        {
            return await _context.Listings
                .Include(l => l.User)
                .ThenInclude(u => u.UserPreference)
                .Include(l => l.ApartmentListing)
                .Where(l => l.IsActive && !l.User.IsDeleted)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Listing> Listings, int TotalCount)>
        SearchListingsAsync(ListingSearchFilters filters)
        {
            IQueryable<Listing> query = _context.Listings
                .Include(l => l.ListingPhotos)
                .Include(l => l.ApartmentListing)
                .Include(l => l.User)
                .AsQueryable();

            // Apply filters
            query = ApplyBasicFilters(query, filters);
            query = ApplyAdvancedFilters(query, filters);

            // Apply FTS
            query = ApplyFullTextSearch(query, filters);

            int totalCount = await query.CountAsync();

            // Sorting
            query = ApplySorting(query, filters);

            // Pagination
            query = ApplyPagination(query, filters);

            var results = await query.ToListAsync();

            return (results, totalCount);
        }

        private IQueryable<Listing> ApplyBasicFilters(IQueryable<Listing> query, ListingSearchFilters filters)
        {
            if (!string.IsNullOrEmpty(filters.City))
                query = query.Where(l => l.City == filters.City);

            if (filters.MinPrice.HasValue)
                query = query.Where(l => l.MonthlyPrice >= filters.MinPrice.Value);

            if (filters.MaxPrice.HasValue)
                query = query.Where(l => l.MonthlyPrice <= filters.MaxPrice.Value);

            if (filters.AvailableRoomsOnly == true)
                query = query.Where(l => l.ApartmentListing.AvailableRooms > 0);

            if (filters.AvailableBedsOnly == true)
                query = query.Where(l => l.ApartmentListing.AvailableBeds > 0);

            return query;
        }

        private IQueryable<Listing> ApplyAdvancedFilters(IQueryable<Listing> query,ListingSearchFilters filters)
        {
            var a = filters;

            if (a.HasElevator.HasValue)
                query = query.Where(l => l.ApartmentListing.HasElevator == a.HasElevator.Value);

            if (a.HasKitchen.HasValue)
                query = query.Where(l => l.ApartmentListing.HasKitchen == a.HasKitchen.Value);

            if (a.HasInternet.HasValue)
                query = query.Where(l => l.ApartmentListing.HasInternet == a.HasInternet.Value);

            if (a.IsPetFriendly.HasValue)
                query = query.Where(l => l.ApartmentListing.IsPetFriendly == a.IsPetFriendly.Value);

            if (a.IsSmokingAllowed.HasValue)
                query = query.Where(l => l.ApartmentListing.IsSmokingAllowed == a.IsSmokingAllowed.Value);

            if (a.MinArea.HasValue)
                query = query.Where(l => l.ApartmentListing.AreaInSqMeter >= a.MinArea.Value);

            if (a.MaxArea.HasValue)
                query = query.Where(l => l.ApartmentListing.AreaInSqMeter <= a.MaxArea.Value);

            if (a.MinRooms.HasValue)
                query = query.Where(l => l.ApartmentListing.TotalRooms >= a.MinRooms.Value);

            if (a.MaxRooms.HasValue)
                query = query.Where(l => l.ApartmentListing.TotalRooms <= a.MaxRooms.Value);

            if (a.MinBeds.HasValue)
                query = query.Where(l => l.ApartmentListing.TotalBeds >= a.MinBeds.Value);

            if (a.MaxBeds.HasValue)
                query = query.Where(l => l.ApartmentListing.TotalBeds <= a.MaxBeds.Value);

            return query;
        }

        private IQueryable<Listing> ApplyFullTextSearch(IQueryable<Listing> query,ListingSearchFilters filters)
        {
            if (string.IsNullOrWhiteSpace(filters.Keyword))
                return query;

            return query.Where(l =>
                EF.Functions.Contains(l.Title, filters.Keyword) ||
                EF.Functions.Contains(l.Description, filters.Keyword)
            );
        }

        private IQueryable<Listing> ApplySorting(IQueryable<Listing> query, ListingSearchFilters filters)
        {
            if (!filters.SortByRelevance || string.IsNullOrWhiteSpace(filters.Keyword))
                return query.OrderByDescending(l => l.CreatedAt);

            return query.OrderByDescending(l =>
                EF.Functions.FreeText(l.Title, filters.Keyword) ||
                EF.Functions.FreeText(l.Description, filters.Keyword)
            );
        }

        private IQueryable<Listing> ApplyPagination(IQueryable<Listing> query,ListingSearchFilters filters)
        {
            int skip = (filters.PageNumber - 1) * filters.PageSize;
            return query.Skip(skip).Take(filters.PageSize);
        }

    }
}
