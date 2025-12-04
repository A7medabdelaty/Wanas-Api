using Wanas.Application.DTOs.Listing;
using Wanas.Domain.Enums;

namespace Wanas.Application.Interfaces
{
    public interface IListingModerationService
    {
        Task<ListingModerationDto?> GetModerationStateAsync(int listingId);
        Task<bool> ModerateAsync(int listingId, ListingModerationStatus newStatus, string adminId, string? note = null);
        Task<bool> FlagAsync(int listingId, string adminId, string reason);
        //Task<bool> UnflagAsync(int listingId, string adminId, string? note = null);
        Task<IEnumerable<ListingPendingDto>> GetPendingAsync();
    }
}