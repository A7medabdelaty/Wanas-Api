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
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;

        public ListingService(IUnitOfWork unit, IMapper mapper, IFileService fileServ)
        {
            this._uow = unit;
            this._mapper = mapper;
            this._fileService = fileServ;
        }

        // add listing photos
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

        public async Task<ListingDetailsDto> CreateListingAsync(CreateListingDto dto, string userId)
        {
            // Map basic listing and apartment
            var listing = _mapper.Map<Listing>(dto);
            listing.UserId = userId;
            listing.IsActive = true;

            listing.ApartmentListing = _mapper.Map<ApartmentListing>(dto);
            listing.ApartmentListing.Listing = listing; // Ensure bidirectional relationship
            listing.ApartmentListing.Rooms ??= new List<Room>();
            listing.ApartmentListing.Rooms.Clear();

            listing.ListingPhotos ??= new HashSet<ListingPhoto>();

            // --- MANUALLY CREATE ROOMS AND BEDS ---
            if (dto.Rooms != null && dto.Rooms.Any())
            {
                foreach (var roomDto in dto.Rooms)
                {
                    var room = new Room
                    {
                        RoomNumber = roomDto.RoomNumber,
                        BedsCount = roomDto.BedsCount,
                        AvailableBeds = roomDto.AvailableBeds,
                        PricePerBed = roomDto.PricePerBed,
                        HasAirConditioner = roomDto.HasAirConditioner,
                        HasFan = roomDto.HasFan,
                        ApartmentListing = listing.ApartmentListing, // Set navigation property for proper tracking
                        Beds = new List<Bed>()
                    };

                    // Create bed entities
                    for (int i = 0; i < roomDto.BedsCount; i++)
                    {
                        room.Beds.Add(new Bed
                        {
                            Room = room // Set navigation property for proper tracking
                        });
                    }

                    listing.ApartmentListing.Rooms.Add(room);
                }
            }

            // --- HANDLE PHOTOS ---
            if (dto.Photos != null && dto.Photos.Any())
            {
                foreach (var file in dto.Photos)
                {
                    string url = await _fileService.SaveFileAsync(file);
                    listing.ListingPhotos.Add(new ListingPhoto { URL = url });
                }
            }

            // Save everything to the database
            await _uow.Listings.AddAsync(listing);
            await _uow.CommitAsync();

            // Reload the listing with all details (rooms and beds) to ensure proper calculation
            var savedListing = await _uow.Listings.GetListingWithDetailsAsync(listing.Id);
            return _mapper.Map<ListingDetailsDto>(savedListing);
        }

        // delete listing
        public async Task<bool> DeleteListingAsync(int id)
        {
            var listing = await _uow.Listings.GetListingWithDetailsAsync(id);
            if (listing == null) return false;

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

        public async Task<IEnumerable<ListingDetailsDto>> GetActiveListingsAsync()
        {
            var listings = await _uow.Listings.GetActiveListingsAsync();
            return _mapper.Map<IEnumerable<ListingDetailsDto>>(listings);
        }

        public async Task<IEnumerable<ListingCardDto>> GetAllListingsAsync()
        {
            var listings = await _uow.Listings.GetAllListingsAsync();
            return _mapper.Map<IEnumerable<ListingCardDto>>(listings);
        }

        public async Task<ListingDetailsDto> GetListingByIdAsync(int id)
        {
            var listing = await _uow.Listings.GetListingWithDetailsAsync(id);
            if (listing == null) return null;

            return _mapper.Map<ListingDetailsDto>(listing);
        }

        public async Task<IEnumerable<ListingDetailsDto>> GetListingsByCityAsync(string city)
        {
            var listings = await _uow.Listings.GetListingsByCityAsync(city);
            return _mapper.Map<IEnumerable<ListingDetailsDto>>(listings);
        }

        public async Task<IEnumerable<ListingDetailsDto>> GetListingsByUserAsync(string userId)
        {
            var listings = await _uow.Listings.GetListingsByUserAsync(userId);
            return _mapper.Map<IEnumerable<ListingDetailsDto>>(listings);
        }

        public async Task<bool> RemovePhotoAsync(int listingId, int photoId, string userId)
        {
            var photo = await _uow.ListingPhotos.GetByIdAsync(photoId);
            if (photo == null || photo.ListingId != listingId || photo.Listing.UserId != userId)
                return false;

            await _fileService.DeleteFileAsync(photo.URL);
            _uow.ListingPhotos.Remove(photo);
            await _uow.CommitAsync();
            return true;
        }

        public async Task<IEnumerable<ListingDetailsDto>> SearchListingsByTitleAsync(string keyword)
        {
            var listings = await _uow.Listings.SearchByTitleAsync(keyword);
            return _mapper.Map<IEnumerable<ListingDetailsDto>>(listings);
        }

        // update listing
        public async Task<ListingDetailsDto> UpdateListingAsync(int id, UpdateListingDto dto)
        {
            // 1️⃣ Load listing with all details
            var listing = await _uow.Listings.GetListingWithDetailsAsync(id);
            if (listing == null) return null;

            // 2️⃣ Update main listing fields (only non-null/has-value fields)
            if (dto.Title != null) listing.Title = dto.Title;
            if (dto.Description != null) listing.Description = dto.Description;
            if (dto.City != null) listing.City = dto.City;
            if (dto.MonthlyPrice.HasValue) listing.MonthlyPrice = dto.MonthlyPrice.Value;

            // 3️⃣ Update ApartmentListing fields (only non-null/has-value fields)
            var apartment = listing.ApartmentListing;
            if (dto.Address != null) apartment.Address = dto.Address;
            if (dto.Floor != null) apartment.Floor = (int) dto.Floor;
            if (dto.AreaInSqMeters.HasValue) apartment.AreaInSqMeters = dto.AreaInSqMeters.Value;
            if (dto.TotalBathrooms.HasValue) apartment.TotalBathrooms = dto.TotalBathrooms.Value;

            if (dto.HasElevator.HasValue) apartment.HasElevator = dto.HasElevator.Value;
            if (dto.HasKitchen.HasValue) apartment.HasKitchen = dto.HasKitchen.Value;
            if (dto.HasInternet.HasValue) apartment.HasInternet = dto.HasInternet.Value;
            if (dto.HasAirConditioner.HasValue) apartment.HasAirConditioner = dto.HasAirConditioner.Value;
            if (dto.HasFans.HasValue) apartment.HasFans = dto.HasFans.Value;
            if (dto.IsPetFriendly.HasValue) apartment.IsPetFriendly = dto.IsPetFriendly.Value;
            if (dto.IsSmokingAllowed.HasValue) apartment.IsSmokingAllowed = dto.IsSmokingAllowed.Value;

            // 4️⃣ Handle rooms
            if (dto.Rooms != null)
            {
                var existingRooms = apartment.Rooms.ToList();

                // a) Update existing rooms
                foreach (var roomDto in dto.Rooms.Where(r => r.Id != 0))
                {
                    var room = existingRooms.FirstOrDefault(r => r.Id == roomDto.Id);
                    if (room != null)
                    {
                        if (roomDto.RoomNumber != null) room.RoomNumber = roomDto.RoomNumber;
                        if (roomDto.BedsCount.HasValue) room.BedsCount = roomDto.BedsCount.Value;
                        if (roomDto.AvailableBeds.HasValue) room.AvailableBeds = roomDto.AvailableBeds.Value;
                        if (roomDto.PricePerBed.HasValue) room.PricePerBed = roomDto.PricePerBed.Value;

                        // Update beds
                        room.Beds.Clear();
                        for (int i = 0; i < room.BedsCount; i++)
                        {
                            room.Beds.Add(new Bed {});
                        }
                    }
                }

                // b) Add new rooms
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
                        newRoom.Beds.Add(new Bed {});
                    }

                    apartment.Rooms.Add(newRoom);
                }

                // c) Remove deleted rooms
                var roomIdsInDto = dto.Rooms.Where(r => r.Id != 0).Select(r => r.Id).ToHashSet();
                var roomsToRemove = existingRooms.Where(r => !roomIdsInDto.Contains(r.Id)).ToList();
                foreach (var room in roomsToRemove)
                {
                    apartment.Rooms.Remove(room);
                }
            }

            // 5️⃣ Handle new photos
            if (dto.NewPhotos != null && dto.NewPhotos.Any())
            {
                listing.ListingPhotos ??= new HashSet<ListingPhoto>();
                foreach (var file in dto.NewPhotos)
                {
                    string url = await _fileService.SaveFileAsync(file);
                    listing.ListingPhotos.Add(new ListingPhoto { URL = url });
                }
            }

            // 6️⃣ Save changes
            _uow.Listings.Update(listing);
            await _uow.CommitAsync();

            // 7️⃣ Return updated listing details DTO
            return _mapper.Map<ListingDetailsDto>(listing);
        }


    }
}
