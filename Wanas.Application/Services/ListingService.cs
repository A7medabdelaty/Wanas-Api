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
                        IsAvailable = roomDto.AvailableBeds > 0,
                        ApartmentListing = listing.ApartmentListing, // Set navigation property for proper tracking
                        Beds = new List<Bed>()
                    };

                    // Create bed entities
                    for (int i = 0; i < roomDto.BedsCount; i++)
                    {
                        room.Beds.Add(new Bed
                        {
                            IsAvailable = i < roomDto.AvailableBeds,
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

            // 2️⃣ Update main listing and apartment listing fields
            _mapper.Map(dto, listing);
            _mapper.Map(dto, listing.ApartmentListing);

            // 3️⃣ Handle rooms
            if (dto.Rooms != null)
            {
                var existingRooms = listing.ApartmentListing.Rooms.ToList();

                // a) Update existing rooms
                foreach (var roomDto in dto.Rooms.Where(r => r.Id != 0))
                {
                    var room = existingRooms.FirstOrDefault(r => r.Id == roomDto.Id);
                    if (room != null)
                    {
                        room.RoomNumber = roomDto.RoomNumber;
                        if (roomDto.BedsCount.HasValue)
                            room.BedsCount = roomDto.BedsCount.Value;
                        if (roomDto.AvailableBeds.HasValue)
                            room.AvailableBeds = roomDto.AvailableBeds.Value;

                        room.PricePerBed = roomDto.PricePerBed ?? room.PricePerBed;
                        room.IsAvailable = room.AvailableBeds > 0;

                        // Update beds
                        room.Beds.Clear();
                        for (int i = 0; i < room.BedsCount; i++)
                        {
                            room.Beds.Add(new Bed { IsAvailable = i < room.AvailableBeds });
                        }
                    }
                }

                // b) Add new rooms
                foreach (var roomDto in dto.Rooms.Where(r => r.Id == 0))
                {
                    var newRoom = new Room
                    {
                        RoomNumber = roomDto.RoomNumber,
                        BedsCount = (int)roomDto.BedsCount,
                        AvailableBeds = (int)roomDto.AvailableBeds,
                        PricePerBed = (decimal)roomDto.PricePerBed,
                        IsAvailable = roomDto.AvailableBeds > 0,
                        Beds = new List<Bed>()
                    };

                    for (int i = 0; i < roomDto.BedsCount; i++)
                    {
                        newRoom.Beds.Add(new Bed { IsAvailable = i < roomDto.AvailableBeds });
                    }

                    listing.ApartmentListing.Rooms.Add(newRoom);
                }

                // c) Remove deleted rooms (rooms in DB but not in DTO)
                var roomIdsInDto = dto.Rooms.Where(r => r.Id != 0).Select(r => r.Id).ToHashSet();
                var roomsToRemove = existingRooms.Where(r => !roomIdsInDto.Contains(r.Id)).ToList();
                foreach (var room in roomsToRemove)
                {
                    listing.ApartmentListing.Rooms.Remove(room);
                }
            }

            // 4️⃣ Handle new photos
            if (dto.NewPhotos != null && dto.NewPhotos.Any())
            {
                listing.ListingPhotos ??= new HashSet<ListingPhoto>();

                foreach (var file in dto.NewPhotos)
                {
                    string url = await _fileService.SaveFileAsync(file);
                    listing.ListingPhotos.Add(new ListingPhoto { URL = url });
                }
            }

            // 5️⃣ Save changes
            _uow.Listings.Update(listing);
            await _uow.CommitAsync();

            // 6️⃣ Return updated listing details DTO
            return _mapper.Map<ListingDetailsDto>(listing);
        }

    }
}
