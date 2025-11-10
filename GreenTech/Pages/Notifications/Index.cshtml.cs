using BLL.Service.Notification.Interface;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Notifications
{
    [AdminAuthorize]
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly INotificationService _notificationService;

        public IndexModel(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var notifications = await _notificationService.GetNotificationsByUserIdAsync(
                userId.Value,
                50
            );
            var unreadCount = await _notificationService.GetUnreadCountByUserIdAsync(userId.Value);

            return new JsonResult(
                new
                {
                    success = true,
                    notifications = notifications.Select(n => new
                    {
                        id = n.Id,
                        title = n.Title,
                        message = n.Message,
                        type = n.Type.ToString(),
                        priority = n.Priority.ToString(),
                        isRead = n.IsRead,
                        createdAt = n.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        referenceId = n.ReferenceId,
                    }),
                    unreadCount = unreadCount,
                }
            );
        }

        public async Task<IActionResult> OnPostMarkAsReadAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return new JsonResult(new { success = false, message = "Unauthorized" })
                {
                    StatusCode = 401,
                };
            }

            var result = await _notificationService.MarkAsReadAsync(id, userId.Value);
            if (!result)
            {
                return new JsonResult(
                    new { success = false, message = "Notification not found or unauthorized" }
                )
                {
                    StatusCode = 404,
                };
            }

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostMarkAllAsReadAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return new JsonResult(new { success = false, message = "Unauthorized" })
                {
                    StatusCode = 401,
                };
            }

            var result = await _notificationService.MarkAllAsReadAsync(userId.Value);
            return new JsonResult(new { success = result });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return new JsonResult(new { success = false, message = "Unauthorized" })
                {
                    StatusCode = 401,
                };
            }

            var result = await _notificationService.DeleteNotificationAsync(id, userId.Value);
            if (!result)
            {
                return new JsonResult(
                    new { success = false, message = "Notification not found or unauthorized" }
                )
                {
                    StatusCode = 404,
                };
            }

            return new JsonResult(new { success = true });
        }
    }
}
