using System.Text.Json;
using GreenTech.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace GreenTech.Pages.Orders
{
    /// <summary>
    /// Handler để nhận cancellation request từ MVC và broadcast qua SignalR đến admin
    /// Không cần authorization vì đây là internal communication
    /// </summary>
    [IgnoreAntiforgeryToken]
    public class ReceiveCancellationRequestModel : PageModel
    {
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly ILogger<ReceiveCancellationRequestModel> _logger;

        public ReceiveCancellationRequestModel(
            IHubContext<OrderHub> hubContext,
            ILogger<ReceiveCancellationRequestModel> logger
        )
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync(
            [FromBody] CancellationRequestNotification notification
        )
        {
            try
            {
                if (notification == null || notification.OrderId == 0)
                {
                    return BadRequest(
                        new { success = false, message = "Invalid notification data" }
                    );
                }

                _logger.LogInformation(
                    "Received cancellation request notification: OrderId={OrderId}, OrderNumber={OrderNumber}",
                    notification.OrderId,
                    notification.OrderNumber
                );

                // Send SignalR notification to all connected clients (admins)
                await _hubContext.Clients.All.SendAsync(
                    "CancellationRequest",
                    new
                    {
                        orderId = notification.OrderId,
                        orderNumber = notification.OrderNumber,
                        userId = notification.UserId,
                        customerName = notification.CustomerName,
                        reason = notification.Reason,
                        message = notification.Message,
                        timestamp = DateTime.UtcNow,
                    }
                );

                // Also send to order-specific group if any admin is viewing that order
                var orderGroup = $"order-{notification.OrderId}";
                await _hubContext
                    .Clients.Group(orderGroup)
                    .SendAsync(
                        "CancellationRequest",
                        new
                        {
                            orderId = notification.OrderId,
                            orderNumber = notification.OrderNumber,
                            userId = notification.UserId,
                            customerName = notification.CustomerName,
                            reason = notification.Reason,
                            message = notification.Message,
                            timestamp = DateTime.UtcNow,
                        }
                    );

                _logger.LogInformation(
                    "Sent SignalR cancellation request notification for OrderId={OrderId}",
                    notification.OrderId
                );

                return new JsonResult(new { success = true, message = "Notification sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending cancellation request notification");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class CancellationRequestNotification
    {
        public int OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public int UserId { get; set; }
        public string? CustomerName { get; set; }
        public string? Reason { get; set; }
        public string? Message { get; set; }
    }
}
