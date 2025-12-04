namespace Wanas.Domain.Entities
{
    public class BedReservationItem
    {
        public int Id { get; set; }
        public int BedReservationId { get; set; }
        public int BedId { get; set; }

        public virtual Reservation BedReservation { get; set; } = null!;
        public virtual Bed Bed { get; set; } = null!;
    }
}
