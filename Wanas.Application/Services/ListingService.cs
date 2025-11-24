using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

            listing.ApartmentListing = mapper.Map<ApartmentListing>(dto);

            foreach (var roomDto in dto.Rooms)
            {
                var room = new Room
                {
                    RoomNumber = roomDto.RoomNumber,
                    BedsCount = roomDto.BedsCount,
                    AvailableBeds = roomDto.AvailableBeds,
                    PricePerBed = roomDto.PricePerBed,
                    IsAvailable = roomDto.AvailableBeds > 0
                };

                if (roomDto.Beds != null)
                {
                    foreach (var bedDto in roomDto.Beds)
                    {
                        room.Beds.Add(new Bed
                        {
                            IsAvailable = bedDto.IsAvailable
                        });
                    }
                }
                else
                {
                    for (int b = 0; b < roomDto.BedsCount; b++)
                    {
                        room.Beds.Add(new Bed { IsAvailable = b < roomDto.AvailableBeds });
                    }
                }

                listing.ApartmentListing.Rooms.Add(room);
            }

            listing.ApartmentListing.TotalRooms = listing.ApartmentListing.Rooms.Count;
            listing.ApartmentListing.AvailableRooms = listing.ApartmentListing.Rooms.Count(r => r.AvailableBeds > 0);
            listing.ApartmentListing.TotalBeds = listing.ApartmentListing.Rooms.Sum(r => r.BedsCount);
            listing.ApartmentListing.AvailableBeds = listing.ApartmentListing.Rooms.Sum(r => r.AvailableBeds);

            if (dto.Photos != null && dto.Photos.Any())
            {
                listing.ListingPhotos = new HashSet<ListingPhoto>();

                foreach (var file in dto.Photos)
                {
                    string url = await fileServ.SaveFileAsync(file);
                    listing.ListingPhotos.Add(new ListingPhoto { URL = url });
                }
            }

            await unit.Listings.AddAsync(listing);
            await unit.CommitAsync();

            return mapper.Map<ListingDetailsDto>(listing);
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

        public async Task<IEnumerable<ListingCardDto>> GetAllListingsAsync()
        {
            var listings = await unit.Listings.GetAllListingsAsync();
            return mapper.Map<IEnumerable<ListingCardDto>>(listings);
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

        public async Task<bool> RemovePhotoAsync(int listingId, int photoId, string userId)
        {
            var photo = await unit.ListingPhotos.GetByIdAsync(photoId);
            if (photo == null || photo.ListingId != listingId || photo.Listing.UserId != userId)
                return false;

            await fileServ.DeleteFileAsync(photo.URL);
            unit.ListingPhotos.Remove(photo);
            await unit.CommitAsync();
            return true;
        }

        public async Task<IEnumerable<ListingDetailsDto>> SearchListingsByTitleAsync(string keyword)
        {
            var listings = await unit.Listings.SearchByTitleAsync(keyword);
            return mapper.Map<IEnumerable<ListingDetailsDto>>(listings);
        }

        public async Task<ListingDetailsDto> UpdateListingAsync(int id, UpdateListingDto dto)
        {
            var listing = await unit.Listings.GetListingWithDetailsAsync(id);
            if (listing == null) return null;

            mapper.Map(dto, listing);
            mapper.Map(dto, listing.ApartmentListing);

            if (dto.Rooms != null)
            {
                foreach (var roomDto in dto.Rooms)
                {
                    var room = listing.ApartmentListing.Rooms.FirstOrDefault(r => r.Id == roomDto.Id);
                    if (room != null)
                    {
                        // Update room aggregates
                        if (roomDto.BedsCount.HasValue)
                            room.BedsCount = roomDto.BedsCount.Value;

                        if (roomDto.AvailableBeds.HasValue)
                        {
                            room.AvailableBeds = roomDto.AvailableBeds.Value;

                            // Update bed availability
                            for (int i = 0; i < room.Beds.Count; i++)
                            {
                                room.Beds.ElementAt(i).IsAvailable = i < room.AvailableBeds;
                            }
                        }

                        if (roomDto.PricePerBed.HasValue)
                            room.PricePerBed = roomDto.PricePerBed.Value;

                        // Update room availability
                        room.IsAvailable = room.AvailableBeds > 0;
                    }
                }
            }

            listing.ApartmentListing.TotalRooms = listing.ApartmentListing.Rooms.Count;
            listing.ApartmentListing.AvailableRooms = listing.ApartmentListing.Rooms.Count(r => r.AvailableBeds > 0);
            listing.ApartmentListing.TotalBeds = listing.ApartmentListing.Rooms.Sum(r => r.BedsCount);
            listing.ApartmentListing.AvailableBeds = listing.ApartmentListing.Rooms.Sum(r => r.AvailableBeds);

            if (dto.NewPhotos != null && dto.NewPhotos.Any())
            {
                foreach (var file in dto.NewPhotos)
                {
                    string url = await fileServ.SaveFileAsync(file);
                    listing.ListingPhotos.Add(new ListingPhoto { URL = url });
                }
            }

            unit.Listings.Update(listing);
            await unit.CommitAsync();

            return mapper.Map<ListingDetailsDto>(listing);
        }

    }
}
