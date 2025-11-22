using AutoMapper;
using Microsoft.AspNetCore.Http;
using Wanas.Application.DTOs.Listing;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class ListingService : IListingService
    {
        private readonly IUnitOfWork unit;
        private readonly IMapper mapper;
        private readonly IFileService fileServ;

        public ListingService(IUnitOfWork unit, IMapper mapper, IFileService fileServ)
        {
            this.unit = unit;
            this.mapper = mapper;
            this.fileServ = fileServ;
        }
        public async Task<CommentDto> AddCommentAsync(CreateCommentDto dto, string userId)
        {
            var comment = mapper.Map<Comment>(dto);
            comment.AuthorId = userId;

            await unit.Comments.AddAsync(comment);
            await unit.CommitAsync();

            return mapper.Map<CommentDto>(comment);
        }

        public async Task AddPhotosToListingAsync(int listingId, List<IFormFile> photos)
        {
            var listing = await unit.Listings.GetByIdAsync(listingId);
            if (listing == null) return;

            foreach (var file in photos)
            {
                var url = await fileServ.SaveFileAsync(file);
                listing.ListingPhotos.Add(new ListingPhoto { URL = url });
            }

            unit.Listings.Update(listing);
            await unit.CommitAsync();
        }

        public async Task<ListingDetailsDto> CreateListingAsync(CreateListingDto dto, string userId)
        {
            var listing = mapper.Map<Listing>(dto);
            listing.UserId = userId;
            listing.IsActive = true;

            if (dto.Photos != null && dto.Photos.Count > 0)
            {
                listing.ListingPhotos = new HashSet<ListingPhoto>();
                foreach (var file in dto.Photos)
                {
                    var url = await fileServ.SaveFileAsync(file);
                    listing.ListingPhotos.Add(new ListingPhoto { URL = url });
                }
            }

            await unit.Listings.AddAsync(listing);
            await unit.CommitAsync();

            return mapper.Map<ListingDetailsDto>(listing);
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var comment = await unit.Comments.GetByIdAsync(commentId);
            if (comment == null) return false;

            unit.Comments.Remove(comment);
            await unit.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteListingAsync(int id)
        {
            var listing = await unit.Listings.GetByIdAsync(id);
            if (listing == null) return false;

            unit.Listings.Remove(listing);
            await unit.CommitAsync();
            return true;
        }

        public async Task<IEnumerable<ListingDetailsDto>> GetActiveListingsAsync()
        {
            var listings = await unit.Listings.GetActiveListingsAsync();
            return mapper.Map<IEnumerable<ListingDetailsDto>>(listings);
        }

        public async Task<ListingDetailsDto> GetListingByIdAsync(int id)
        {
            var listing = await unit.Listings.GetListingWithDetailsAsync(id);
            if (listing == null) return null;

            return mapper.Map<ListingDetailsDto>(listing);
        }

        public async Task<IEnumerable<ListingDetailsDto>> GetListingsByCityAsync(string city)
        {
            var listings = await unit.Listings.GetListingsByCityAsync(city);
            return mapper.Map<IEnumerable<ListingDetailsDto>>(listings);
        }

        public async Task<IEnumerable<ListingDetailsDto>> GetListingsByUserAsync(string userId)
        {
            var listings = await unit.Listings.GetListingsByUserAsync(userId);
            return mapper.Map<IEnumerable<ListingDetailsDto>>(listings);
        }

        public async Task<bool> RemovePhotoAsync(int photoId)
        {
            var photo = await unit.ListingPhotos.GetByIdAsync(photoId);
            if (photo == null) return false;

            unit.ListingPhotos.Remove(photo);
            await unit.CommitAsync();
            return true;
        }

        public async Task<IEnumerable<ListingDetailsDto>> SearchListingsByTitleAsync(string keyword)
        {
            var listings = await unit.Listings.SearchByTitleAsync(keyword);
            return mapper.Map<IEnumerable<ListingDetailsDto>>(listings);
        }

        public async Task<CommentDto> UpdateCommentAsync(int commentId, UpdateCommentDto dto)
        {
            var comment = await unit.Comments.GetByIdAsync(commentId);
            if (comment == null) return null;

            mapper.Map(dto, comment);

            unit.Comments.Update(comment);
            await unit.CommitAsync();

            return mapper.Map<CommentDto>(comment);
        }

        public async Task<ListingDetailsDto> UpdateListingAsync(int id, UpdateListingDto dto)
        {
            var listing = await unit.Listings.GetListingWithDetailsAsync(id);
            if (listing == null) return null;

            mapper.Map(dto, listing);
            mapper.Map(dto, listing.ApartmentListing);

            if (dto.NewPhotos != null && dto.NewPhotos.Count > 0)
            {
                foreach (var file in dto.NewPhotos)
                {
                    var url = await fileServ.SaveFileAsync(file);
                    listing.ListingPhotos.Add(new ListingPhoto { URL = url });
                }
            }

            unit.Listings.Update(listing);
            await unit.CommitAsync();

            return mapper.Map<ListingDetailsDto>(listing);
        }
    }
}
