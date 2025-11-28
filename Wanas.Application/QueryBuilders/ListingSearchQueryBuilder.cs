using Wanas.Application.DTOs.Search;
using Wanas.Domain.Entities;

namespace Wanas.Application.QueryBuilders
{
        public static class ListingSearchQueryBuilder
        {
            public static IQueryable<Listing> ApplyFilters(
                IQueryable<Listing> query,
                ListingSearchRequestDto request)
            {
                if (!string.IsNullOrWhiteSpace(request.City))
                    query = query.Where(x => x.City == request.City);

                if (request.MinPrice.HasValue)
                    query = query.Where(x => x.MonthlyPrice >= request.MinPrice.Value);

                if (request.MaxPrice.HasValue)
                    query = query.Where(x => x.MonthlyPrice <= request.MaxPrice.Value);

                if (request.HasInternet == true)
                    query = query.Where(x => x.ApartmentListing.HasInternet);

                if (request.HasKitchen == true)
                    query = query.Where(x => x.ApartmentListing.HasKitchen);

                if (request.HasElevator == true)
                    query = query.Where(x => x.ApartmentListing.HasElevator);

                return query;
            }

            public static IQueryable<Listing> ApplySorting(
                IQueryable<Listing> query, string? sortBy)
            {
                return sortBy?.ToLower() switch
                {
                    "price" => query.OrderBy(x => x.MonthlyPrice),
                    "price_desc" => query.OrderByDescending(x => x.MonthlyPrice),
                    "newest" => query.OrderByDescending(x => x.CreatedAt),
                    _ => query.OrderByDescending(x => x.CreatedAt),
                };
            }

        }
    }
