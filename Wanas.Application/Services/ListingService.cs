using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

        // DELETE LISTING
        public async Task<bool> DeleteListingAsync(int id)
        {
            var listing = await _uow.Listings.GetListingWithDetailsTrackedAsync(id);
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

        // PAGED LISTINGS
        public async Task<ApiPagedResponse<ListingCardDto>> GetPagedListingsAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _uow.Listings.GetPagedListingsAsync(pageNumber, pageSize);
            var mapped = _mapper.Map<IEnumerable<ListingCardDto>>(items);
            return new ApiPagedResponse<ListingCardDto>(mapped, totalCount, pageNumber, pageSize);
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

                    // Regenerate beds
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
                    apartment.Rooms.Remove(room);
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

    }
}
