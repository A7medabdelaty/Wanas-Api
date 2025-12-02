using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
    public interface IBedReservationRepository : IGenericRepository<BedReservation>
    {
        Task<BedReservation?> GetByIdWithItemsAsync(int reservationId);
        Task<IEnumerable<BedReservation>> GetActiveReservationsForBedsAsync(IEnumerable<int> bedIds);
        Task<BedReservation?> GetActiveReservationForListingAsync(int listingId);
    }
}
