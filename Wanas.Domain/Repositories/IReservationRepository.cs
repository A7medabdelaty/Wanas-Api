using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
    public interface IReservationRepository : IGenericRepository<Reservation>
    {
        Task<Reservation> GetFullReservationAsync(int id);
        Task<Reservation?> GetReservationWithBedsAsync(int id);
        Task<List<Reservation>> GetReservationsByOwnerAsync(string ownerId);
        Task<List<Reservation>> GetByOwnerAsync(string ownerId);
        Task<List<Reservation>> GetByRenterAsync(string renterId);
    }
}
