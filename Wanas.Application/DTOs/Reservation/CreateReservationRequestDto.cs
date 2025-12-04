
namespace Wanas.Application.DTOs.Reservation
{
    public class CreateReservationRequestDto
    {
        public int ListingId { get; set; }
        public List<int> BedIds { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
