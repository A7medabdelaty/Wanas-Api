using Microsoft.AspNetCore.Http;
using Wanas.Application.DTOs.Listing;

namespace Wanas.Application.Interfaces
{ 
    public interface IListingService
    {
        Task<IEnumerable<ListingCardDto>> GetAllListingsAsync();
        Task<ListingDetailsDto> GetListingByIdAsync(int id);
        Task<IEnumerable<ListingDetailsDto>> GetListingsByUserAsync(string userId);
        Task<IEnumerable<ListingDetailsDto>> GetListingsByCityAsync(string city);
        Task<IEnumerable<ListingDetailsDto>> GetActiveListingsAsync();
        Task<IEnumerable<ListingDetailsDto>> SearchListingsByTitleAsync(string keyword);
        Task<ListingDetailsDto> CreateListingAsync(CreateListingDto dto, string userId);
        Task<ListingDetailsDto> UpdateListingAsync(int id, UpdateListingDto dto);
        Task<bool> DeleteListingAsync(int id);

        // Listing Photos
        Task AddPhotosToListingAsync(int listingId, List<IFormFile> photos);
        Task<bool> RemovePhotoAsync(int listingId, int photoId, string userId);
    }

}
