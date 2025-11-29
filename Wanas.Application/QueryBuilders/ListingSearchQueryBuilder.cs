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
            // City
            if (!string.IsNullOrWhiteSpace(request.City))
                query = query.Where(x => x.City == request.City);

            // Price
            if (request.MinPrice.HasValue)
                query = query.Where(x => x.MonthlyPrice >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(x => x.MonthlyPrice <= request.MaxPrice.Value);

            // Rooms (use EF.Count)
            if (request.MinRooms.HasValue)
                query = query.Where(x => x.ApartmentListing.Rooms.Count >= request.MinRooms);

            if (request.MaxRooms.HasValue)
                query = query.Where(x => x.ApartmentListing.Rooms.Count <= request.MaxRooms);

            // Beds (use EF.Count)
            if (request.MinBeds.HasValue)
                query = query.Where(x => x.ApartmentListing.Beds.Count >= request.MinBeds);

            if (request.MaxBeds.HasValue)
                query = query.Where(x => x.ApartmentListing.Beds.Count <= request.MaxBeds);

            // Area
            if (request.MinArea.HasValue)
                query = query.Where(x => x.ApartmentListing.AreaInSqMeters >= request.MinArea);

            if (request.MaxArea.HasValue)
                query = query.Where(x => x.ApartmentListing.AreaInSqMeters <= request.MaxArea);

            // Floor
            if (request.MinFloor.HasValue)
                query = query.Where(x => x.ApartmentListing.Floor >= request.MinFloor);

            if (request.MaxFloor.HasValue)
                query = query.Where(x => x.ApartmentListing.Floor <= request.MaxFloor);

            // Availability (EF-translatable)
            if (request.OnlyAvailable == true)
            {
                query = query.Where(x =>
                    x.ApartmentListing.Rooms.Any(r =>
                        r.Beds.Any(b => b.RenterId == null)
                    )
                );
            }

            // Features
            if (request.HasInternet == true)
                query = query.Where(x => x.ApartmentListing.HasInternet);

            if (request.HasKitchen == true)
                query = query.Where(x => x.ApartmentListing.HasKitchen);

            if (request.HasElevator == true)
                query = query.Where(x => x.ApartmentListing.HasElevator);

            if (request.HasAirConditioner == true)
                query = query.Where(x => x.ApartmentListing.HasAirConditioner);

            if (request.HasFans == true)
                query = query.Where(x => x.ApartmentListing.HasFans);

            if (request.IsPetFriendly == true)
                query = query.Where(x => x.ApartmentListing.IsPetFriendly);

            if (request.IsSmokingAllowed == true)
                query = query.Where(x => x.ApartmentListing.IsSmokingAllowed);

            return query;
        }

        public static IQueryable<Listing> ApplySorting(
            IQueryable<Listing> query, string? sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "price" or "price_asc" => query.OrderBy(x => x.MonthlyPrice),
                "price_desc" => query.OrderByDescending(x => x.MonthlyPrice),

                "area" or "area_asc" => query.OrderBy(x => x.ApartmentListing.AreaInSqMeters),
                "area_desc" => query.OrderByDescending(x => x.ApartmentListing.AreaInSqMeters),

                "floor" or "floor_asc" => query.OrderBy(x => x.ApartmentListing.Floor),
                "floor_desc" => query.OrderByDescending(x => x.ApartmentListing.Floor),

                "oldest" => query.OrderBy(x => x.CreatedAt),
                "newest" => query.OrderByDescending(x => x.CreatedAt),

                _ => query.OrderByDescending(x => x.CreatedAt),
            };
        }
    }
}
