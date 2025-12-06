using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using Wanas.Application.DTOs.Listing;
using Wanas.Application.Interfaces;
using Wanas.Domain.Entities;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class ListingService : IListingService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;

        public ListingService(IUnitOfWork unit, IMapper mapper, IFileService fileServ)
        {
            this._uow = unit;
            this._mapper = mapper;
            this._fileService = fileServ;
        }

        // Add photos to a listing
        public async Task AddPhotosToListingAsync(int listingId, List<IFormFile> photos)
        {
            var listing = await _uow.Listings.GetByIdAsync(listingId);
            if (listing == null)
                return;

            listing.ListingPhotos ??= new HashSet<ListingPhoto>();

            foreach (var file in photos)
            {
                var url = await _fileService.SaveFileAsync(file);
                listing.ListingPhotos.Add(new ListingPhoto { URL = url });
            }

            _uow.Listings.Update(listing);
            await _uow.CommitAsync();
        }

        // CREATE LISTING
        public async Task<ListingDetailsDto> CreateListingAsync(CreateListingDto dto, string userId)
        {
            // ---------- Create Main Listing ----------
            var listing = new Listing
            {
                Title = dto.Title,
                Description = dto.Description,
                City = dto.City,
                MonthlyPrice = dto.MonthlyPrice,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                UserId = userId,
                ApartmentListing = new ApartmentListing
                {
                    Address = dto.Address,
                    MonthlyPrice = dto.MonthlyPrice,
                    HasElevator = dto.HasElevator,
                    Floor = dto.Floor,
                    AreaInSqMeters = dto.AreaInSqMeters,
                    TotalBathrooms = dto.TotalBathrooms,
                    HasKitchen = dto.HasKitchen,
                    HasInternet = dto.HasInternet,
                    HasAirConditioner = dto.HasAirConditioner,
                    HasFans = dto.HasFans,
                    IsPetFriendly = dto.IsPetFriendly,
                    IsSmokingAllowed = dto.IsSmokingAllowed,
                    Rooms = new List<Room>()
                },
                ListingPhotos = new HashSet<ListingPhoto>()
            };

            // ---------- Rooms & Beds ----------
            if (dto.Rooms != null)
            {
                foreach (var roomDto in dto.Rooms)
                {
                    var room = new Room
                    {
                        RoomNumber = roomDto.RoomNumber,
                        BedsCount = roomDto.BedsCount,
                        PricePerBed = roomDto.PricePerBed,
                        HasAirConditioner = roomDto.HasAirConditioner,
                        HasFan = roomDto.HasFan,
                        Beds = new List<Bed>()
                    };

                    // Auto-generate Beds
                    for (int i = 0; i < roomDto.BedsCount; i++)
                    {
                        room.Beds.Add(new Bed
                        {
                            IsAvailable = true
                        });
                    }

                    room.RecalculateAvailability();
                    listing.ApartmentListing.Rooms.Add(room);
                }
            }

            // ---------- Save Photos ----------
            if (dto.Photos != null)
            {
                foreach (var file in dto.Photos)
                {
                    var url = await _fileService.SaveFileAsync(file);
                    listing.ListingPhotos.Add(new ListingPhoto { URL = url });
                }
            }

            // ---------- Save Listing (generate Listing.Id) ----------
            await _uow.Listings.AddAsync(listing);
            await _uow.CommitAsync();

            // ---------- Create Group Chat ----------
            var groupChat = new Chat
            {
                IsGroup = true,
                Name = listing.Title,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Chats.AddAsync(groupChat);
            await _uow.CommitAsync();

            listing.GroupChatId = groupChat.Id;
            await _uow.CommitAsync();

            // ---------- Load full listing ----------
            var savedListing = await _uow.Listings.GetListingWithDetailsAsync(listing.Id);
            return _mapper.Map<ListingDetailsDto>(savedListing);
        }

        // DELETE LISTING
        public async Task<bool> DeleteListingAsync(int id)
        {
            var listing = await _uow.Listings.GetListingWithDetailsAsync(id);
            if (listing == null)
                return false;

            if (listing.ListingPhotos != null)
            {
                foreach (var photo in listing.ListingPhotos)
                {
                    await _fileService.DeleteFileAsync(photo.URL);
                }
            }

            _uow.Listings.Remove(listing);
            await _uow.CommitAsync();
            return true;
        }

        // ACTIVE LISTINGS
        public async Task<IEnumerable<ListingDetailsDto>> GetActiveListingsAsync()
        {
            var listings = await _uow.Listings.GetActiveListingsAsync();
            return _mapper.Map<IEnumerable<ListingDetailsDto>>(listings);
        }

        // ALL LISTINGS
        public async Task<IEnumerable<ListingCardDto>> GetAllListingsAsync()
        {
            var listings = await _uow.Listings.GetAllListingsAsync();
            return _mapper.Map<IEnumerable<ListingCardDto>>(listings);
        }

        // GET BY ID
        public async Task<ListingDetailsDto> GetListingByIdAsync(int id)
        {
            var listing = await _uow.Listings.GetListingWithDetailsAsync(id);
            if (listing == null)
                return null;

            return _mapper.Map<ListingDetailsDto>(listing);
        }

        // GET BY CITY
        public async Task<IEnumerable<ListingDetailsDto>> GetListingsByCityAsync(string city)
        {
            var listings = await _uow.Listings.GetListingsByCityAsync(city);
            return _mapper.Map<IEnumerable<ListingDetailsDto>>(listings);
        }

        // GET BY USER
        public async Task<IEnumerable<ListingDetailsDto>> GetListingsByUserAsync(string userId)
        {
            var listings = await _uow.Listings.GetListingsByUserAsync(userId);
            return _mapper.Map<IEnumerable<ListingDetailsDto>>(listings);
        }

        

        // REMOVE PHOTO
        public async Task<bool> RemovePhotoAsync(int listingId, int photoId, string userId)
        {
            var photo = await _uow.ListingPhotos.GetPhotoWithListingByIdAsync(photoId);
            if (photo == null || photo.ListingId != listingId || photo.Listing.UserId != userId)
                return false;

            await _fileService.DeleteFileAsync(photo.URL);
            _uow.ListingPhotos.Remove(photo);
            await _uow.CommitAsync();
            return true;
        }

        // SEARCH BY TITLE
        public async Task<IEnumerable<ListingDetailsDto>> SearchListingsByTitleAsync(string keyword)
        {
            var listings = await _uow.Listings.SearchByTitleAsync(keyword);
            return _mapper.Map<IEnumerable<ListingDetailsDto>>(listings);
        }

        // UPDATE LISTING
        public async Task<ListingDetailsDto> UpdateListingAsync(int id, UpdateListingDto dto)
        {
            var listing = await _uow.Listings.GetListingWithDetailsAsync(id);
            if (listing == null)
                return null;

            // Update main listing fields
            if (dto.Title != null)
                listing.Title = dto.Title;
            if (dto.Description != null)
                listing.Description = dto.Description;
            if (dto.City != null)
                listing.City = dto.City;
            if (dto.MonthlyPrice.HasValue)
                listing.MonthlyPrice = dto.MonthlyPrice.Value;

            var apartment = listing.ApartmentListing;

            if (dto.Address != null)
                apartment.Address = dto.Address;
            if (dto.Floor != null)
                apartment.Floor = (int) dto.Floor;
            if (dto.AreaInSqMeters.HasValue)
                apartment.AreaInSqMeters = dto.AreaInSqMeters.Value;
            if (dto.TotalBathrooms.HasValue)
                apartment.TotalBathrooms = dto.TotalBathrooms.Value;

            if (dto.HasElevator.HasValue)
                apartment.HasElevator = dto.HasElevator.Value;
            if (dto.HasKitchen.HasValue)
                apartment.HasKitchen = dto.HasKitchen.Value;
            if (dto.HasInternet.HasValue)
                apartment.HasInternet = dto.HasInternet.Value;
            if (dto.HasAirConditioner.HasValue)
                apartment.HasAirConditioner = dto.HasAirConditioner.Value;
            if (dto.HasFans.HasValue)
                apartment.HasFans = dto.HasFans.Value;
            if (dto.IsPetFriendly.HasValue)
                apartment.IsPetFriendly = dto.IsPetFriendly.Value;
            if (dto.IsSmokingAllowed.HasValue)
                apartment.IsSmokingAllowed = dto.IsSmokingAllowed.Value;

            // Rooms update
            if (dto.Rooms != null)
            {
                var existingRooms = apartment.Rooms.ToList();

                // Update existing rooms
                foreach (var roomDto in dto.Rooms.Where(r => r.Id != 0))
                {
                    var room = existingRooms.FirstOrDefault(r => r.Id == roomDto.Id);
                    if (room != null)
                    {
                        if (roomDto.RoomNumber != null)
                            room.RoomNumber = roomDto.RoomNumber;
                        if (roomDto.BedsCount.HasValue)
                            room.BedsCount = roomDto.BedsCount.Value;
                        if (roomDto.AvailableBeds.HasValue)
                            room.AvailableBeds = roomDto.AvailableBeds.Value;
                        if (roomDto.PricePerBed.HasValue)
                            room.PricePerBed = roomDto.PricePerBed.Value;

                        room.Beds.Clear();
                        for (int i = 0; i < room.BedsCount; i++)
                        {
                            room.Beds.Add(new Bed
                            {
                                IsAvailable = i < room.AvailableBeds
                            });
                        }

                        room.RecalculateAvailability();
                    }
                }

                // Add new rooms
                foreach (var roomDto in dto.Rooms.Where(r => r.Id == 0))
                {
                    var newRoom = new Room
                    {
                        RoomNumber = roomDto.RoomNumber,
                        BedsCount = roomDto.BedsCount ?? 0,
                        AvailableBeds = roomDto.AvailableBeds ?? 0,
                        PricePerBed = roomDto.PricePerBed ?? 0,
                        Beds = new List<Bed>()
                    };

                    for (int i = 0; i < newRoom.BedsCount; i++)
                    {
                        newRoom.Beds.Add(new Bed
                        {
                            IsAvailable = i < newRoom.AvailableBeds
                        });
                    }

                    newRoom.RecalculateAvailability();
                    apartment.Rooms.Add(newRoom);
                }

                // Remove deleted rooms
                var roomIdsInDto = dto.Rooms
                    .Where(r => r.Id != 0)
                    .Select(r => r.Id)
                    .ToHashSet();

                var roomsToRemove = existingRooms
                    .Where(r => !roomIdsInDto.Contains(r.Id))
                    .ToList();

                foreach (var room in roomsToRemove)
                {
                    apartment.Rooms.Remove(room);
                }
            }

            // Add new photos
            if (dto.NewPhotos != null && dto.NewPhotos.Any())
            {
                listing.ListingPhotos ??= new HashSet<ListingPhoto>();
                foreach (var file in dto.NewPhotos)
                {
                    var url = await _fileService.SaveFileAsync(file);
                    listing.ListingPhotos.Add(new ListingPhoto { URL = url });
                }
            }

            _uow.Listings.Update(listing);
            await _uow.CommitAsync();

            return _mapper.Map<ListingDetailsDto>(listing);
        }
    }
}
