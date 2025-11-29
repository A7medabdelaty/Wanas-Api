namespace Wanas.Domain.Entities
{
    public class Room
    {
        public int Id { get; set; }
        public int RoomNumber { get; set; }
        public int BedsCount { get; set; }
        public int AvailableBeds { get; set; }
        public decimal PricePerBed { get; set; }
        public bool HasAirConditioner { get; set; }
        public bool HasFan { get; set; }
        public bool IsAvailable => Beds.Any(b => b.IsAvailable);
        public virtual ICollection<Bed> Beds { get; set; } = new List<Bed>();
        public int ApartmentListingId { get; set; }
        public ApartmentListing ApartmentListing { get; set; }
    }
}
