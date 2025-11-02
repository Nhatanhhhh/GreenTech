using System.Text.Json;
using GreenTech.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace GreenTech.Pages.Orders
{
    /// <summary>
    /// Handler để nhận new order notification từ MVC và broadcast qua SignalR đến admin
    /// Không cần authorization vì đây là internal communication
    /// </summary>
    [IgnoreAntiforgeryToken]
    public class ReceiveNewOrderModel : PageModel
    {
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly ILogger<ReceiveNewOrderModel> _logger;

        public ReceiveNewOrderModel(
            IHubContext<OrderHub> hubContext,
            ILogger<ReceiveNewOrderModel> logger
        )
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync([FromBody] NewOrderNotification notification)
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
                    "Received new order notification: OrderId={OrderId}, OrderNumber={OrderNumber}, UserId={UserId}",
                    notification.OrderId,
                    notification.OrderNumber,
                    notification.UserId
                );

                // Broadcast to all admins (anyone viewing order list)
                await _hubContext.Clients.All.SendAsync(
                    "NewOrderCreated",
                    new
                    {
                        orderId = notification.OrderId,
                        orderNumber = notification.OrderNumber,
                        userId = notification.UserId,
                        customerName = notification.CustomerName,
                        total = notification.Total,
                        status = notification.Status,
                        message = notification.Message,
                        timestamp = DateTime.UtcNow,
                    }
                );

                _logger.LogInformation(
                    "Sent SignalR new order notification for OrderId={OrderId}",
                    notification.OrderId
                );

                return new JsonResult(new { success = true, message = "Notification sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending new order notification");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class NewOrderNotification
    {
        public int OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public int UserId { get; set; }
        public string? CustomerName { get; set; }
        public decimal Total { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
    }
}
