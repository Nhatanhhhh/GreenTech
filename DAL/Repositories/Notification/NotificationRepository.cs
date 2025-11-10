using DAL.Context;
using DAL.Models;
using DAL.Repositories.Notification.Interface;
using Microsoft.EntityFrameworkCore;
using NotificationModel = DAL.Models.Notification;

namespace DAL.Repositories.Notification
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationModel?> GetByIdAsync(int notificationId)
        {
            return await _context
                .Notifications.Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == notificationId);
        }

        public async Task<IEnumerable<NotificationModel>> GetByUserIdAsync(
            int userId,
            int? limit = null
        )
        {
            var query = _context
                .Notifications.Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            if (limit.HasValue)
            {
                query = (IOrderedQueryable<NotificationModel>)query.Take(limit.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<NotificationModel>> GetUnreadByUserIdAsync(int userId)
        {
            return await _context
                .Notifications.Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountByUserIdAsync(int userId)
        {
            return await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<NotificationModel> CreateAsync(NotificationModel notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<NotificationModel> UpdateAsync(NotificationModel notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context
                .Notifications.Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
                return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAllByUserIdAsync(int userId)
        {
            var notifications = await _context
                .Notifications.Where(n => n.UserId == userId)
                .ToListAsync();

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
