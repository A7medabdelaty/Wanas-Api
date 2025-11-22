using AutoMapper;
using Wanas.Application.DTOs.Listing;
using Wanas.Application.Interfaces;
using Wanas.Domain.Enums;
using Wanas.Domain.Repositories;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Wanas.Application.Services
{
    public class ListingModerationService : IListingModerationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _audit;
        public ListingModerationService(IUnitOfWork uow, IMapper mapper, IAuditLogService audit)
        {
            _uow = uow; _mapper = mapper; _audit = audit;
        }
        public async Task<ListingModerationDto?> GetModerationStateAsync(int listingId)
        {
            var listing = await _uow.Listings.GetByIdAsync(listingId);
            return listing == null ? null : _mapper.Map<ListingModerationDto>(listing);
        }
        public async Task<bool> ModerateAsync(int listingId, ListingModerationStatus newStatus, string adminId, string? note = null)
        {
            var listing = await _uow.Listings.GetByIdAsync(listingId);
            if (listing == null) return false;
            listing.ModerationStatus = newStatus;
            listing.ModeratedByAdminId = adminId;
            listing.ModeratedAt = DateTime.UtcNow;
            if (note != null) listing.ModerationNote = note;
            _uow.Listings.Update(listing);
            await _uow.CommitAsync();
            await _audit.LogAsync("ListingModerated", adminId, listing.UserId, $"ListingId={listingId}; Status={newStatus}; Note={note}");
            return true;
        }
        public async Task<bool> FlagAsync(int listingId, string adminId, string reason)
        {
            var listing = await _uow.Listings.GetByIdAsync(listingId);
            if (listing == null) return false;
            listing.IsFlagged = true;
            listing.FlagReason = reason;
            listing.ModeratedByAdminId = adminId;
            listing.ModeratedAt = DateTime.UtcNow;
            _uow.Listings.Update(listing);
            await _uow.CommitAsync();
            await _audit.LogAsync("ListingFlagged", adminId, listing.UserId, $"ListingId={listingId}; Reason={reason}");
            return true;
        }
        public async Task<bool> UnflagAsync(int listingId, string adminId, string? note = null)
        {
            var listing = await _uow.Listings.GetByIdAsync(listingId);
            if (listing == null) return false;
            listing.IsFlagged = false;
            listing.FlagReason = null;
            listing.ModeratedByAdminId = adminId;
            listing.ModeratedAt = DateTime.UtcNow;
            if (note != null) listing.ModerationNote = note;
            _uow.Listings.Update(listing);
            await _uow.CommitAsync();
            await _audit.LogAsync("ListingUnflagged", adminId, listing.UserId, $"ListingId={listingId}; Note={note}");
            return true;
        }
        public async Task<IEnumerable<ListingModerationDto>> GetPendingAsync()
        {
            var pending = await _uow.Listings.FindAsync(l => l.ModerationStatus == ListingModerationStatus.Pending);
            return pending.Select(l => _mapper.Map<ListingModerationDto>(l));
        }
    }
}