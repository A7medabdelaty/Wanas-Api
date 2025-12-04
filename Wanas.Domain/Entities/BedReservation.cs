namespace Wanas.Domain.Entities
{
    public class BedReservation
    {
        public int BedId { get; set; }
        public Bed Bed { get; set; }

        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }
    }

}
