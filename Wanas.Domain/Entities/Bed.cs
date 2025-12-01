namespace Wanas.Domain.Entities
{
    public class Bed
    {
        public int Id { get; set; }
        public bool IsAvailable { get; set; }
        public int RoomId { get; set; }
        public string? RenterId { get; set; }
        public Room Room { get; set; }
        public ApplicationUser? Renter { get; set; }
        public void SetRenter(string? renterId)
        {
            RenterId = renterId;
            IsAvailable = renterId == null;
        }

    }
}
