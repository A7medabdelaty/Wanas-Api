namespace Wanas.Application.DTOs.Listing
{
    public class CreateRoomDto
    {
        public int RoomNumber { get; set; }
        public int BedsCount { get; set; }
        public int AvailableBeds { get; set; }
        public decimal PricePerBed { get; set; }
        public bool HasAirConditioner { get; set; }
        public bool HasFan { get; set; }
        public List<BedDto> Beds { get; set; }
    }
}
