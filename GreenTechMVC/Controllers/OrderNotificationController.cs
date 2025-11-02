using GreenTechMVC.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GreenTechMVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderNotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly ILogger<OrderNotificationController> _logger;

        public OrderNotificationController(
            IHubContext<NotificationHub> notificationHub,
            ILogger<OrderNotificationController> logger
        )
        {
            _notificationHub = notificationHub;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint để Razor Pages gọi khi có order update
        /// </summary>
        [HttpPost("order-updated")]
        public async Task<IActionResult> OrderUpdated(
            [FromBody] OrderUpdateNotification notification
        )
        {
            try
            {
                if (notification == null || notification.OrderId == 0)
                {
                    return BadRequest("Invalid notification data");
                }

                _logger.LogInformation(
                    "Received order update notification: OrderId={OrderId}, Status={Status}, UserId={UserId}, OrderNumber={OrderNumber}",
                    notification.OrderId,
                    notification.Status,
                    notification.UserId,
                    notification.OrderNumber
                );

                var orderUpdateData = new
                {
                    orderId = notification.OrderId,
                    orderNumber = notification.OrderNumber,
                    status = notification.Status,
                    statusText = notification.StatusText,
                    message = notification.Message,
                    timestamp = DateTime.UtcNow,
                };

                // Gửi notification đến user group (cho Index page và Details page)
                if (notification.UserId > 0)
                {
                    var userGroup = $"user-{notification.UserId}";
                    _logger.LogInformation(
                        "Sending SignalR notification to user group '{UserGroup}' for order {OrderId}",
                        userGroup,
                        notification.OrderId
                    );

                    await _notificationHub
                        .Clients.Group(userGroup)
                        .SendAsync("OrderUpdated", orderUpdateData);

                    _logger.LogInformation(
                        "✓ Sent SignalR notification to user group '{UserGroup}' for order {OrderId}",
                        userGroup,
                        notification.OrderId
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "Cannot send notification to user group - UserId is invalid: {UserId}",
                        notification.UserId
                    );
                }

                // Gửi notification đến order group (cho Details page đang xem order cụ thể)
                var orderGroup = $"order-{notification.OrderId}";
                _logger.LogInformation(
                    "Sending SignalR notification to order group '{OrderGroup}' for order {OrderId}",
                    orderGroup,
                    notification.OrderId
                );

                await _notificationHub
                    .Clients.Group(orderGroup)
                    .SendAsync("OrderUpdated", orderUpdateData);

                _logger.LogInformation(
                    "✓ Sent SignalR notification to order group '{OrderGroup}' for order {OrderId}",
                    orderGroup,
                    notification.OrderId
                );

                return Ok(
                    new
                    {
                        success = true,
                        message = "Notification sent",
                        orderId = notification.OrderId,
                        userId = notification.UserId,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order update notification");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint để Razor Pages gọi khi có shipping tracking update
        /// </summary>
        [HttpPost("shipping-updated")]
        public async Task<IActionResult> ShippingUpdated(
            [FromBody] ShippingUpdateNotification notification
        )
        {
            try
            {
                if (notification == null || notification.OrderId == 0)
                {
                    return BadRequest("Invalid notification data");
                }

                _logger.LogInformation(
                    "Received shipping update notification: OrderId={OrderId}, Status={Status}, UserId={UserId}",
                    notification.OrderId,
                    notification.Status,
                    notification.UserId
                );

                // Gửi notification đến user group
                if (notification.UserId > 0)
                {
                    var userGroup = $"user-{notification.UserId}";
                    await _notificationHub
                        .Clients.Group(userGroup)
                        .SendAsync(
                            "ShippingUpdated",
                            new
                            {
                                orderId = notification.OrderId,
                                orderNumber = notification.OrderNumber,
                                status = notification.Status,
                                location = notification.Location,
                                description = notification.Description,
                                trackingNumber = notification.TrackingNumber,
                                message = notification.Message,
                                timestamp = DateTime.UtcNow,
                            }
                        );
                }

                // Gửi notification đến order group
                var orderGroup = $"order-{notification.OrderId}";
                await _notificationHub
                    .Clients.Group(orderGroup)
                    .SendAsync(
                        "ShippingUpdated",
                        new
                        {
                            orderId = notification.OrderId,
                            orderNumber = notification.OrderNumber,
                            status = notification.Status,
                            location = notification.Location,
                            description = notification.Description,
                            trackingNumber = notification.TrackingNumber,
                            message = notification.Message,
                            timestamp = DateTime.UtcNow,
                        }
                    );

                return Ok(new { success = true, message = "Notification sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending shipping update notification");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class OrderUpdateNotification
    {
        public int OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public int UserId { get; set; }
        public string? Status { get; set; }
        public string? StatusText { get; set; }
        public string? Message { get; set; }
    }

    public class ShippingUpdateNotification
    {
        public int OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public int UserId { get; set; }
        public string? Status { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Message { get; set; }
    }
}
