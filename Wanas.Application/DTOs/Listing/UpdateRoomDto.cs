namespace Wanas.Application.DTOs.Listing
{
    public class UpdateRoomDto
    {
        public int Id { get; set; }
        public int RoomNumber { get; set; } 
        public int? BedsCount { get; set; }
        public int? AvailableBeds { get; set; }
        public decimal? PricePerBed { get; set; }
        public bool HasAirConditioner { get; set; }

    }
}
