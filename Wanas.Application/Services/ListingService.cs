using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Wanas.Application.DTOs.Listing;
using Wanas.Application.Interfaces;
using Wanas.Application.Responses;
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;

namespace Wanas.Application.Services
{
    public class ListingService : IListingService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly IChromaIndexingService _chromaIndexingService;
        private readonly ILogger<ListingService> _logger;
        private readonly IReviewRepository _reviewRepository;

        public ListingService(
            IUnitOfWork unit,
            IMapper mapper,
            IFileService fileServ,
            IChromaIndexingService chromaIndexingService,
            ILogger<ListingService> logger,
            IReviewRepository reviewRepository)
        {
            this._uow = unit;
            this._mapper = mapper;
            this._fileService = fileServ;
            this._chromaIndexingService = chromaIndexingService;
            this._logger = logger;
            this._reviewRepository = reviewRepository;
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

            // Reindex after photos added
            _ = Task.Run(async () =>
            {
                try
                {
                    await _chromaIndexingService.IndexListingAsync(listingId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to reindex listing {ListingId} after adding photos", listingId);
                }
            });
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
                CreatedAt = DateTime.UtcNow,
                ChatParticipants = new List<ChatParticipant>
                {
                    new ChatParticipant
                    {
                        UserId = userId,
                        IsAdmin = true,
                        JoinedAt = DateTime.UtcNow
                    }
                }
            };

            await _uow.Chats.AddAsync(groupChat);
            await _uow.CommitAsync();

            listing.GroupChatId = groupChat.Id;
            await _uow.CommitAsync();

            // ---------- Index to ChromaDB in background ----------
            var listingId = listing.Id;
            _ = Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation("Background indexing listing {ListingId} to ChromaDB", listingId);
                    await _chromaIndexingService.IndexListingAsync(listingId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to index listing {ListingId} to ChromaDB", listingId);
                }
            });

            // ---------- Load full listing ----------
            var savedListing = await _uow.Listings.GetListingWithDetailsAsync(listing.Id);
            return _mapper.Map<ListingDetailsDto>(savedListing);
        }

        public async Task<bool> DeleteListingAsync(int id)
        {
            var listing = await _uow.Listings.GetListingWithDetailsTrackedAsync(id);
            if (listing == null)
                return false;

            // ck if there are any occupied beds (meaning active reservations/tenants)
            var beds = await _uow.Beds.GetBedsByListingIdAsync(id);
            bool hasOccupiedBeds = beds.Any(b => b.RenterId != null);

            if (hasOccupiedBeds)
            {
                // Mark as inactive instead of deleting
                listing.IsActive = false;
                _uow.Listings.Update(listing);
                await _uow.CommitAsync();

                // Remove from ChromaDB index in background as it is now inactive
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _chromaIndexingService.RemoveListingFromIndexAsync(id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to remove inactive listing {ListingId} from ChromaDB index", id);
                    }
                });

                return true;
            }

            // 1. Safely delete associated photos (Ignore file lock errors)
            if (listing.ListingPhotos != null)
            {
                foreach (var photo in listing.ListingPhotos)
                {
                    try
                    {
                        await _fileService.DeleteFileAsync(photo.URL);
                    }
                    catch (Exception ex)
                    {
                        
                        _logger.LogWarning("Could not delete file {Url}: {Message}", photo.URL, ex.Message);
                    }
                }
            }

            // 2. Delete group chat if it exists
            if (listing.GroupChatId.HasValue)
            {
                var groupChat = await _uow.Chats.GetByIdAsync(listing.GroupChatId.Value);
                if (groupChat != null)
                {
                    _uow.Chats.Remove(groupChat);
                }
            }

            // 3. Delete all 1-1 private chats linked to this listing
            var privateChats = await _uow.Chats.FindAsync(c => c.ListingId == id);
            if (privateChats != null && privateChats.Any())
            {
                foreach (var chat in privateChats)
                {
                    _uow.Chats.Remove(chat);
                }
            }

           
            // 5. Delete the listing
            _uow.Listings.Remove(listing);

            // 6. Commit everything
            await _uow.CommitAsync();

            // 7. Remove from ChromaDB index in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _chromaIndexingService.RemoveListingFromIndexAsync(id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove listing {ListingId} from ChromaDB index", id);
                }
            });

            return true;
        }
        

        public async Task<IEnumerable<ListingDetailsDto>> GetActiveListingsAsync()
        {
            var listings = await _uow.Listings.GetActiveListingsAsync();
            var listingDtos = _mapper.Map<IEnumerable<ListingDetailsDto>>(listings);

            foreach (var dto in listingDtos)
            {
                dto.AverageRating = await _reviewRepository.GetAverageRatingAsync(dto.Id.ToString());
                 // Check for occupied beds
                var beds = await _uow.Beds.GetBedsByListingIdAsync(dto.Id);
                dto.HasOccupiedBeds = beds.Any(b => b.RenterId != null);
            }

            return listingDtos;
        }


        // ALL LISTINGS
        public async Task<IEnumerable<ListingDetailsDto>> GetAllListingsAsync()
        {
            var listings = await _uow.Listings.GetAllListingsAsync();
            var listingDtos = _mapper.Map<IEnumerable<ListingDetailsDto>>(listings);

            foreach (var dto in listingDtos)
            {
                dto.AverageRating = await _reviewRepository.GetAverageRatingAsync(dto.Id.ToString());
                 // Check for occupied beds
                var beds = await _uow.Beds.GetBedsByListingIdAsync(dto.Id);
                dto.HasOccupiedBeds = beds.Any(b => b.RenterId != null);
            }

            return listingDtos;
        }

        // PAGED LISTINGS
        public async Task<ApiPagedResponse<ListingDetailsDto>> GetPagedListingsAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _uow.Listings.GetPagedActiveListingsAsync(pageNumber, pageSize);
            var mapped = _mapper.Map<IEnumerable<ListingDetailsDto>>(items);

            foreach (var dto in mapped)
            {
                dto.AverageRating = await _reviewRepository.GetAverageRatingAsync(dto.Id.ToString());
                 // Check for occupied beds
                var beds = await _uow.Beds.GetBedsByListingIdAsync(dto.Id);
                dto.HasOccupiedBeds = beds.Any(b => b.RenterId != null);
            }

            return new ApiPagedResponse<ListingDetailsDto>(mapped, totalCount, pageNumber, pageSize);
        }

        // GET BY ID
        public async Task<ListingDetailsDto> GetListingByIdAsync(int id)
        {
            var listing = await _uow.Listings.GetListingWithDetailsAsync(id);
            if (listing == null)
                return null;

            var dto = _mapper.Map<ListingDetailsDto>(listing);

            dto.AverageRating = await _reviewRepository.GetAverageRatingAsync(listing.Id.ToString());

            // Check for occupied beds
            var beds = await _uow.Beds.GetBedsByListingIdAsync(listing.Id);
            dto.HasOccupiedBeds = beds.Any(b => b.RenterId != null);

            // Map Tenants
            if (listing.ApartmentListing?.Rooms != null)
            {
                var tenants = listing.ApartmentListing.Rooms
                    .SelectMany(r => r.Beds)
                    .Where(b => b.Renter != null)
                    .Select(b => b.Renter)
                    .Distinct() // ApplicationUser implements generic equality or by reference? Better Use DistinctBy if possible or GroupBy
                    .GroupBy(u => u.Id).Select(g => g.First())
                    .Select(u => new TenantDto
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Photo = u.Photo,
                        Gender = u.Gender,
                        Age = u.Age,
                        Bio = u.Bio
                    })
                    .ToList();
                
                dto.Tenants = tenants;
            }

            return dto;
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
            var dtos = _mapper.Map<IEnumerable<ListingDetailsDto>>(listings);

            foreach (var dto in dtos)
            {
                dto.AverageRating = await _reviewRepository.GetAverageRatingAsync(dto.Id.ToString());
                 // Check for occupied beds
                var beds = await _uow.Beds.GetBedsByListingIdAsync(dto.Id);
                dto.HasOccupiedBeds = beds.Any(b => b.RenterId != null);
            }

            return dtos;
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
            var listing = await _uow.Listings.GetListingWithDetailsTrackedAsync(id);
            if (listing == null)
                return null;

            // Update Listing (simple patch)
            listing.Title = dto.Title ?? listing.Title;
            listing.Description = dto.Description ?? listing.Description;
            listing.City = dto.City ?? listing.City;
            listing.MonthlyPrice = dto.MonthlyPrice ?? listing.MonthlyPrice;

            var apartment = listing.ApartmentListing;

            apartment.Address = dto.Address ?? apartment.Address;
            apartment.Floor = dto.Floor ?? apartment.Floor;
            apartment.AreaInSqMeters = dto.AreaInSqMeters ?? apartment.AreaInSqMeters;
            apartment.TotalBathrooms = dto.TotalBathrooms ?? apartment.TotalBathrooms;

            apartment.HasElevator = dto.HasElevator ?? apartment.HasElevator;
            apartment.HasKitchen = dto.HasKitchen ?? apartment.HasKitchen;
            apartment.HasInternet = dto.HasInternet ?? apartment.HasInternet;
            apartment.HasAirConditioner = dto.HasAirConditioner ?? apartment.HasAirConditioner;
            apartment.HasFans = dto.HasFans ?? apartment.HasFans;
            apartment.IsPetFriendly = dto.IsPetFriendly ?? apartment.IsPetFriendly;
            apartment.IsSmokingAllowed = dto.IsSmokingAllowed ?? apartment.IsSmokingAllowed;


            // ---------- ROOMS UPDATE (Simplified) ----------
            if (dto.Rooms != null)
            {
                var existingRooms = apartment.Rooms.ToList();

                // Update existing
                foreach (var rDto in dto.Rooms.Where(r => r.Id != 0))
                {
                    var room = existingRooms.FirstOrDefault(x => x.Id == rDto.Id);
                    if (room == null)
                        continue;

                    room.RoomNumber = rDto.RoomNumber;
                    room.BedsCount = rDto.BedsCount ?? room.BedsCount;
                    room.AvailableBeds = rDto.AvailableBeds ?? room.AvailableBeds;
                    room.PricePerBed = rDto.PricePerBed ?? room.PricePerBed;

                    var existingBeds = room.Beds.OrderBy(b => b.Id).ToList();
                    int newBedCount = rDto.BedsCount ?? room.BedsCount;

                    // 1. Update existing beds (check if we need to remove some)
                    // 1. Update existing beds (check if we need to remove some)
                    if (existingBeds.Count > newBedCount)
                    {
                        int countToRemove = existingBeds.Count - newBedCount;
                        
                        // Valid candidates for removal: Beds with NO Renter AND NO Reservations
                        var removableBeds = existingBeds
                            .Where(b => b.RenterId == null && (!b.BedReservations?.Any() ?? true))
                            .Take(countToRemove)
                            .ToList();

                        foreach (var bed in removableBeds)
                        {
                            _uow.Beds.Remove(bed);
                            room.Beds.Remove(bed);
                        }
                        
                        // If we still need to remove beds but all remaining are reserved/occupied:
                        // We do NOTHING (or log/warn). We do NOT remove reserved beds.
                    }

                    // 2. Add new beds if needed
                    else if (existingBeds.Count < newBedCount)
                    {
                        int bedsToAdd = newBedCount - existingBeds.Count;
                        for (int i = 0; i < bedsToAdd; i++)
                        {
                            room.Beds.Add(new Bed { IsAvailable = true });
                        }
                    }

                    // 3. Update availability logic (optional, dependent on your business logic)
                    // room.AvailableBeds might come from DTO, but usually calculated based on reservations.
                    // If DTO sends AvailableBeds, we might need to set IsAvailable on specific beds? 
                    // For now, simpler approach: Update logic relies on RecalculateAvailability later.
                    
                    room.RecalculateAvailability();
                }

                // Add new rooms
                foreach (var rDto in dto.Rooms.Where(r => r.Id == 0))
                {
                    var room = new Room
                    {
                        RoomNumber = rDto.RoomNumber,
                        BedsCount = rDto.BedsCount ?? 0,
                        AvailableBeds = rDto.AvailableBeds ?? 0,
                        PricePerBed = rDto.PricePerBed ?? 0,
                        Beds = new List<Bed>()
                    };

                    for (int i = 0; i < room.BedsCount; i++)
                    {
                        room.Beds.Add(new Bed
                        {
                            IsAvailable = i < room.AvailableBeds
                        });
                    }

                    room.RecalculateAvailability();
                    apartment.Rooms.Add(room);
                }

                // Delete rooms
                var incomingIds = dto.Rooms.Where(r => r.Id != 0).Select(r => r.Id).ToHashSet();
                foreach (var room in existingRooms.Where(r => !incomingIds.Contains(r.Id)))
                {
                    // Safety Check: Cannot delete room if it has occupied beds
                    if (room.Beds.Any(b => b.RenterId != null || b.BedReservations.Any()))
                    {
                        // We choose to throw here to alert the caller (frontend) that their request is invalid/dangerous
                        // preventing accidental data loss or corruption.
                        throw new InvalidOperationException($"Cannot delete Room {room.RoomNumber} because it contains occupied or reserved beds.");
                    }
                    apartment.Rooms.Remove(room);
                }
            }

            if (dto.NewPhotos != null)
            {
                listing.ListingPhotos ??= new HashSet<ListingPhoto>();
                foreach (var file in dto.NewPhotos)
                {
                    var url = await _fileService.SaveFileAsync(file);
                    listing.ListingPhotos.Add(new ListingPhoto { URL = url });
                }
            }

            await _uow.CommitAsync();

            return _mapper.Map<ListingDetailsDto>(listing);
        }

        public async Task<bool> ReactivateListingAsync(int id, string userId)
        {
             var listing = await _uow.Listings.GetByIdAsync(id);
             if (listing == null) return false;
             if (listing.UserId != userId) throw new UnauthorizedAccessException("Not owner");

             // Check if there is at least one available bed
             var availableBeds = await _uow.Beds.GetAvailableBedsByListingAsync(id);
             
             // Check if all beds are truly occupied or reserved, logic similar to PayDepositAsync but simplified
             // If GetAvailableBedsByListingAsync returns beds that are free to book, then we can reactivate.
             if (availableBeds != null && availableBeds.Any())
             {
                 listing.IsActive = true;
                 _uow.Listings.Update(listing);
                 await _uow.CommitAsync();

                 // Re-index to ChromaDB
                 _ = Task.Run(async () =>
                 {
                    try
                    {
                        await _chromaIndexingService.IndexListingAsync(id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to reindex reactivated listing {ListingId}", id);
                    }
                 });

                 return true;
             }
             
             return false;
        }

    }
}
