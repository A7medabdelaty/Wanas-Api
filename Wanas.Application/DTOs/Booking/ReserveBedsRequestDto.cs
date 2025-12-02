namespace Wanas.Application.DTOs.Booking
{
    public class ReserveBedsRequestDto
    {
        public int ListingId { get; set; }
        public List<int> BedIds { get; set; } = new();
    }

    public class ReserveBedsResponseDto
    {
        public int ReservationId { get; set; }
        public int ListingId { get; set; }
        public IEnumerable<int> ReservedBedIds { get; set; } = Enumerable.Empty<int>();
        public DateTime ReservedAt { get; set; }
        public bool IsConfirmed { get; set; }
    }

    public class ConfirmReservationRequestDto
    {
        public int ReservationId { get; set; }
        public string? PaymentReference { get; set; }
    }
}
