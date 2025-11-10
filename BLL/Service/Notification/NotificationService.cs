using BLL.Service.Notification.Interface;
using DAL.Models;
using DAL.Models.Enum;
using DAL.Repositories.Notification.Interface;
using Microsoft.Extensions.Logging;
using NotificationModel = DAL.Models.Notification;

namespace BLL.Service.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            ILogger<NotificationService> logger
        )
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<NotificationModel>> GetNotificationsByUserIdAsync(
            int userId,
            int? limit = null
        )
        {
            return await _notificationRepository.GetByUserIdAsync(userId, limit);
        }

        public async Task<IEnumerable<NotificationModel>> GetUnreadNotificationsByUserIdAsync(
            int userId
        )
        {
            return await _notificationRepository.GetUnreadByUserIdAsync(userId);
        }

        public async Task<int> GetUnreadCountByUserIdAsync(int userId)
        {
            return await _notificationRepository.GetUnreadCountByUserIdAsync(userId);
        }

        public async Task<NotificationModel> CreateNotificationAsync(
            int userId,
            string title,
            string message,
            NotificationType type,
            NotificationPriority priority = NotificationPriority.MEDIUM,
            int? referenceId = null
        )
        {
            var notification = new NotificationModel
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                Priority = priority,
                ReferenceId = referenceId,
                IsRead = false,
                CreatedAt = DateTime.Now,
            };

            return await _notificationRepository.CreateAsync(notification);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null || notification.UserId != userId)
            {
                return false;
            }

            return await _notificationRepository.MarkAsReadAsync(notificationId);
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            return await _notificationRepository.MarkAllAsReadAsync(userId);
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null || notification.UserId != userId)
            {
                return false;
            }

            return await _notificationRepository.DeleteAsync(notificationId);
        }
    }
}
