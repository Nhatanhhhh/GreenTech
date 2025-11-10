using DAL.Models;
using DAL.Models.Enum;

namespace DAL.Repositories.Notification.Interface
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(int notificationId);
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, int? limit = null);
        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId);
        Task<int> GetUnreadCountByUserIdAsync(int userId);
        Task<Notification> CreateAsync(Notification notification);
        Task<Notification> UpdateAsync(Notification notification);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteAsync(int notificationId);
        Task<bool> DeleteAllByUserIdAsync(int userId);
    }
}
