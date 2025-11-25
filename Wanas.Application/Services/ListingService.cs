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
            var listing = _mapper.Map<Listing>(dto);
            listing.UserId = userId;
            listing.IsActive = true;

            listing.ApartmentListing = _mapper.Map<ApartmentListing>(dto);
            listing.ApartmentListing.Rooms ??= new List<Room>();
            listing.ListingPhotos ??= new HashSet<ListingPhoto>();

            // rooms
            foreach (var roomDto in dto.Rooms)
            {
                var room = new Room
                {
                    RoomNumber = roomDto.RoomNumber,
                    BedsCount = roomDto.BedsCount,
                    AvailableBeds = roomDto.AvailableBeds,
                    PricePerBed = roomDto.PricePerBed,
                    IsAvailable = roomDto.AvailableBeds > 0,
                    Beds = new List<Bed>()
                };

                if (roomDto.Beds != null)
                {
                    foreach (var bedDto in roomDto.Beds)
                    {
                        room.Beds.Add(new Bed { IsAvailable = bedDto.IsAvailable });
                    }
                }
                else
                {
                    for (int i = 0; i < roomDto.BedsCount; i++)
                    {
                        room.Beds.Add(new Bed
                        {
                            IsAvailable = i < roomDto.AvailableBeds
                        });
                    }
                }

                listing.ApartmentListing.Rooms.Add(room);
            }

            UpdateApartmentListingAggregates(listing);

            // photos
            if (dto.Photos != null && dto.Photos.Any())
            {
                listing.ListingPhotos = new HashSet<ListingPhoto>();

                foreach (var file in dto.Photos)
                {
                    string url = await _fileService.SaveFileAsync(file);
                    listing.ListingPhotos.Add(new ListingPhoto { URL = url });
                }
            }

            await _uow.Listings.AddAsync(listing);
            await _uow.CommitAsync();

            return _mapper.Map<ListingDetailsDto>(listing);
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

        // update listing data
        public async Task<ListingDetailsDto> UpdateListingAsync(int id, UpdateListingDto dto)
        {
            var listing = await _uow.Listings.GetListingWithDetailsAsync(id);
            if (listing == null) return null;

            _mapper.Map(dto, listing);
            _mapper.Map(dto, listing.ApartmentListing);

            // rooms update
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

            UpdateApartmentListingAggregates(listing);

            if (dto.NewPhotos != null && dto.NewPhotos.Any())
            {
                foreach (var file in dto.NewPhotos)
                {
                    string url = await _fileService.SaveFileAsync(file);
                    listing.ListingPhotos.Add(new ListingPhoto { URL = url });
                }
            }

            _uow.Listings.Update(listing);
            await _uow.CommitAsync();

            return _mapper.Map<ListingDetailsDto>(listing);
        }

        // helper method to calculate listing aggregates data
        private void UpdateApartmentListingAggregates(Listing listing)
        {
            var apt = listing.ApartmentListing;

            apt.TotalRooms = apt.Rooms.Count;
            apt.AvailableRooms = apt.Rooms.Count(r => r.AvailableBeds > 0);
            apt.TotalBeds = apt.Rooms.Sum(r => r.BedsCount);
            apt.AvailableBeds = apt.Rooms.Sum(r => r.AvailableBeds);
        }
    }
}
