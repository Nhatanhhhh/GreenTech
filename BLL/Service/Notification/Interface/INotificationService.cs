using DAL.Models;

namespace BLL.Service.Notification.Interface
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(
            int userId,
            int? limit = null
        );
        Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(int userId);
        Task<int> GetUnreadCountByUserIdAsync(int userId);
        Task<Notification> CreateNotificationAsync(
            int userId,
            string title,
            string message,
            DAL.Models.Enum.NotificationType type,
            DAL.Models.Enum.NotificationPriority priority =
                DAL.Models.Enum.NotificationPriority.MEDIUM,
            int? referenceId = null
        );
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteNotificationAsync(int notificationId, int userId);
    }
}
