namespace Wanas.Domain.Entities
{
    public class Listing
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public decimal MonthlyPrice { get; set; }
        public int TotalRooms { get; set; }
        public int TotalBeds { get; set; }
        public int AvailableRooms { get; set; }
        public int AvailableBeds { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string? OwnerId { get; set; }
        public HashSet<Room> Rooms { get; set; } = new();
        public HashSet<ListingPhoto> ListingPhotos { get; set; } = new();
        public ApplicationUser Owner { get; set; }
        public HashSet<Payment> Payments { get; set; } = new();
        public ICollection<Match> Matches { get; set; }
    }
}
