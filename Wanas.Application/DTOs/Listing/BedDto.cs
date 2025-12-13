namespace Wanas.Application.DTOs.Listing
{
    public class BedDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int ListingId { get; set; }
        public bool IsAvailable { get; set; }
        public decimal PricePerBed { get; set; }
        
        // For reservation display
        public string RenterId { get; set; }
        public string BedNumber { get; set; }
        public int RoomNumber { get; set; }
    }
}
