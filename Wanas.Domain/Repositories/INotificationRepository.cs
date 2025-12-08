using System.Collections.Generic;
using System.Threading.Tasks;
using Wanas.Domain.Entities;

namespace Wanas.Domain.Repositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(string userId);
        Task AddAsync(Notification notification);
    }
}
