namespace Wanas.Domain.Entities
{
    public class Bed
    {
        public int Id { get; set; }
        public Boolean IsAvailable { get; set; }
        public int RoomId { get; set; }
        public int? RenterId { get; set; }
        public Room Room { get; set; }
        public ApplicationUser? Renter { get; set; }
    }
}
