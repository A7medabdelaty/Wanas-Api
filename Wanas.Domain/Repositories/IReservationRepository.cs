using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
    public interface IReservationRepository : IGenericRepository<Reservation>
    {
        Task<Reservation?> GetFullReservationAsync(int reservationId);
    }
}
