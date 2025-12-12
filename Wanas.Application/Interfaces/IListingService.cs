using Microsoft.AspNetCore.Http;
using Wanas.Application.DTOs.Listing;
using Wanas.Application.Responses;
using Wanas.Domain.Entities;

namespace Wanas.Application.Interfaces
{ 
    public interface IListingService
    {
        Task<IEnumerable<ListingCardDto>> GetAllListingsAsync();
        Task<ApiPagedResponse<ListingDetailsDto>> GetPagedListingsAsync(int pageNumber, int pageSize);
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
