using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wanas.Domain.Enums;
using Wanas.Infrastructure.Persistence;
namespace Wanas.Application.Services
{
    public class ReservationExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ReservationExpirationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDBContext>();

                var expirationTime = DateTime.UtcNow.AddMinutes(-30);

                var expiredReservations = await db.Reservations
                    .Include(r => r.Beds)
                    .Where(r => r.PaymentStatus == PaymentStatus.Pending &&
                                r.CreatedAt < expirationTime)
                    .ToListAsync();

                if (expiredReservations.Any())
                {
                    foreach (var r in expiredReservations)
                    {
                        r.PaymentStatus = PaymentStatus.Expired;

                        // FREE BEDS
                        foreach (var br in r.Beds)
                        {
                            var bed = await db.Beds.FindAsync(br.BedId);
                            if (bed != null)
                            {
                                bed.RenterId = null;
                                bed.IsAvailable = true;
                            }
                        }
                    }

                    await db.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
