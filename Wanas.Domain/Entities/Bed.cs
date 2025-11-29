namespace Wanas.Domain.Entities
{
    public class Bed
    {
        public int Id { get; set; }
        public bool IsAvailable => RenterId == null;
        public int RoomId { get; set; }
        public string? RenterId { get; set; }
        public Room Room { get; set; }
        public ApplicationUser? Renter { get; set; }
    }
}
