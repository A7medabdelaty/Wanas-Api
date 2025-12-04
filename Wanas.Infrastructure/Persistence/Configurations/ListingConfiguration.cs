using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wanas.Domain.Entities;

namespace Wanas.Infrastructure.Persistence.Configurations
{
    public class ListingConfiguration : IEntityTypeConfiguration<Listing>
    {
        public void Configure(EntityTypeBuilder<Listing> builder)
        {
            // Primary fields
            builder.HasKey(l => l.Id);
            builder.Property(l => l.Title).HasMaxLength(150);
            builder.Property(l => l.Description).HasMaxLength(2000);
            builder.Property(l => l.City).HasMaxLength(50);
            builder.Property(l => l.IsActive).HasColumnType("bit");

            builder
                .HasIndex(l => l.City)
                .HasDatabaseName("IX_Listing_City");

            builder.Property(l => l.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationship: Listing → User (many listings per user)
            builder.HasOne(l => l.User)
                .WithMany(u => u.Listings)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: Listing → ApartmentListing (one-to-one)
            builder.HasOne(l => l.ApartmentListing)
                .WithOne(a => a.Listing)
                .HasForeignKey<ApartmentListing>(a => a.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: Listing → Photos
            builder.HasMany(l => l.ListingPhotos)
                .WithOne(p => p.Listing)
                .HasForeignKey(p => p.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: Listing → Payments
            builder.HasMany(l => l.Payments)
                .WithOne(p => p.Listing)
                .HasForeignKey(p => p.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: Listing → Comments
            builder.HasMany(l => l.Comments)
                .WithOne(c => c.Listing)
                .HasForeignKey(c => c.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many chats per listing
            builder.HasMany(l => l.Chats)
                .WithOne(c => c.Listing)
                .HasForeignKey(c => c.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            // One group chat per listing
            builder.HasOne(l => l.GroupChat)
                .WithMany()
                .HasForeignKey(l => l.GroupChatId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
