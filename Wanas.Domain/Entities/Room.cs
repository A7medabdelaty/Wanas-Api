
namespace Wanas.Domain.Entities
{
    public class Room
    {
        public int Id { get; set; }
        public int RoomNumber { get; set; }
        public int BedsCount { get; set; }
        public int AvailableBeds { get; set; }
        public decimal PricePerBed { get; set; }
        public Boolean IsAvailable { get; set; }
        public int? ListingId { get; set; }
        public HashSet<Bed> Beds { get; set; } = new();
        public  Listing Listing { get; set; }
    }
}
