
namespace Wanas.Application.DTOs.Reservation
{
    public class CreateReservationRequestDto
    {
        public int ListingId { get; set; }
        public List<int> BedIds { get; set; }

        public DateTime StartDate { get; set; }

        public int DurationInDays { get; set; }
    }
}
