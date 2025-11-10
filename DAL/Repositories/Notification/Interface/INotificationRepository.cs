using DAL.Models;
using DAL.Models.Enum;
using NotificationModel = DAL.Models.Notification;

namespace DAL.Repositories.Notification.Interface
{
    public interface INotificationRepository
    {
        Task<NotificationModel?> GetByIdAsync(int notificationId);
        Task<IEnumerable<NotificationModel>> GetByUserIdAsync(int userId, int? limit = null);
        Task<IEnumerable<NotificationModel>> GetUnreadByUserIdAsync(int userId);
        Task<int> GetUnreadCountByUserIdAsync(int userId);
        Task<NotificationModel> CreateAsync(NotificationModel notification);
        Task<NotificationModel> UpdateAsync(NotificationModel notification);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteAsync(int notificationId);
        Task<bool> DeleteAllByUserIdAsync(int userId);
    }
}
