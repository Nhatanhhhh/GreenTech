using GreenTechMVC.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GreenTechMVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserNotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly ILogger<UserNotificationController> _logger;

        public UserNotificationController(
            IHubContext<NotificationHub> notificationHub,
            ILogger<UserNotificationController> logger
        )
        {
            _notificationHub = notificationHub;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint để Razor Pages gọi khi ban/unban user
        /// </summary>
        [HttpPost("user-banned")]
        public async Task<IActionResult> UserBanned([FromBody] UserBanNotification notification)
        {
            try
            {
                if (notification == null || notification.UserId == 0)
                {
                    return BadRequest(
                        new { success = false, message = "Invalid notification data" }
                    );
                }

                _logger.LogInformation(
                    "Received user ban notification: UserId={UserId}, IsBanned={IsBanned}, FullName={FullName}",
                    notification.UserId,
                    notification.IsBanned,
                    notification.FullName
                );

                var banData = new
                {
                    userId = notification.UserId,
                    isBanned = notification.IsBanned,
                    fullName = notification.FullName,
                    message = notification.IsBanned
                        ? $"Tài khoản của bạn đã bị chặn bởi quản trị viên."
                        : $"Tài khoản của bạn đã được bỏ chặn.",
                    timestamp = DateTime.UtcNow,
                };

                // Gửi notification đến user group
                var userGroup = $"user-{notification.UserId}";
                _logger.LogInformation(
                    "Sending SignalR notification to user group '{UserGroup}' for user ban",
                    userGroup
                );

                await _notificationHub.Clients.Group(userGroup).SendAsync("UserBanned", banData);

                _logger.LogInformation(
                    "✓ Sent SignalR notification to user group '{UserGroup}' for user ban",
                    userGroup
                );

                return new JsonResult(new { success = true, message = "Notification sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending user ban notification");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class UserBanNotification
    {
        public int UserId { get; set; }
        public bool IsBanned { get; set; }
        public string? FullName { get; set; }
        public string? Message { get; set; }
    }
}
