using System.Text;
using System.Text.Json;
using BLL.Service.Order.Interface;
using DAL.DTOs.Order;
using DAL.Models.Enum;
using GreenTech.Filters;
using GreenTech.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace GreenTech.Pages.Orders
{
    [AdminAuthorize]
    public class DetailsModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<OrderHub> _orderHub;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(
            IOrderService orderService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IHubContext<OrderHub> orderHub,
            ILogger<DetailsModel> logger
        )
        {
            _orderService = orderService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _orderHub = orderHub;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public OrderResponseDTO? Order { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Order = await _orderService.GetOrderDetailsAsync(Id);
                if (Order == null)
                {
                    return NotFound();
                }
                return Page();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(UpdateOrderStatusDTO model)
        {
            // Normalize empty Reason to null (field is optional)
            if (string.IsNullOrWhiteSpace(model.Reason))
            {
                model.Reason = null;
            }

            // Remove ModelState errors for Reason if it's empty/null (since it's optional)
            if (ModelState.ContainsKey("Reason") && string.IsNullOrWhiteSpace(model.Reason))
            {
                ModelState["Reason"]?.Errors.Clear();
            }

            if (!ModelState.IsValid)
            {
                Order = await _orderService.GetOrderDetailsAsync(model.OrderId);
                return Page();
            }

            try
            {
                // Get current user ID from session
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue)
                {
                    model.UpdatedBy = userId.Value;
                }

                var updatedOrder = await _orderService.UpdateOrderStatusAsync(model);
                TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công";

                // Broadcast real-time update via SignalR (both to admin and customer)
                await BroadcastOrderUpdateAsync(
                    updatedOrder,
                    model.Status.ToString(),
                    GetStatusText(model.Status)
                );

                // Send realtime notification to MVC via HTTP
                await SendOrderUpdateNotificationAsync(
                    updatedOrder,
                    model.Status.ToString(),
                    GetStatusText(model.Status)
                );

                return RedirectToPage("./Details", new { id = model.OrderId });
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
                Order = await _orderService.GetOrderDetailsAsync(model.OrderId);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostApproveCancelAsync(
            int orderId,
            string? reason = null
        )
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");

                // Get order to check if it has cancellation request
                var order = await _orderService.GetOrderDetailsAsync(orderId);
                if (order == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng";
                    return RedirectToPage("./Details", new { id = orderId });
                }

                if (string.IsNullOrWhiteSpace(order.CancelledReason))
                {
                    TempData["Error"] = "Đơn hàng này chưa có yêu cầu hủy";
                    return RedirectToPage("./Details", new { id = orderId });
                }

                // Update order status to CANCELLED
                // UpdateOrderStatusAsync will automatically restore product quantities
                var updateDTO = new UpdateOrderStatusDTO
                {
                    OrderId = orderId,
                    Status = OrderStatus.CANCELLED,
                    Reason = reason ?? order.CancelledReason + " (Đã được admin duyệt)",
                    UpdatedBy = userId,
                };

                var updatedOrder = await _orderService.UpdateOrderStatusAsync(updateDTO);

                // Calculate refund amount (only subtotal, not shipping fee)
                var refundAmount = Math.Min(order.WalletAmountUsed, order.Subtotal);
                var refundMessage =
                    refundAmount > 0
                        ? $"Đã duyệt hủy đơn hàng thành công. Số lượng sản phẩm đã được khôi phục. Đã hoàn lại {refundAmount:N0} ₫ vào ví (chỉ tiền sản phẩm, không bao gồm phí vận chuyển)."
                        : "Đã duyệt hủy đơn hàng thành công. Số lượng sản phẩm đã được khôi phục.";

                TempData["Success"] = refundMessage;

                // Broadcast real-time update via SignalR
                await BroadcastOrderUpdateAsync(
                    updatedOrder,
                    OrderStatus.CANCELLED.ToString(),
                    "Đã hủy"
                );

                // Send realtime notification to MVC customer via HTTP
                await SendOrderUpdateNotificationAsync(
                    updatedOrder,
                    OrderStatus.CANCELLED.ToString(),
                    "Đã hủy"
                );

                return RedirectToPage("./Details", new { id = orderId });
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToPage("./Details", new { id = orderId });
            }
        }

        public async Task<IActionResult> OnPostRejectCancelAsync(int orderId, string? reason = null)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");

                // Get order before rejecting
                var order = await _orderService.GetOrderDetailsAsync(orderId);
                if (order == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng";
                    return RedirectToPage("./Details", new { id = orderId });
                }

                if (string.IsNullOrWhiteSpace(order.CancelledReason))
                {
                    TempData["Error"] = "Đơn hàng này chưa có yêu cầu hủy";
                    return RedirectToPage("./Details", new { id = orderId });
                }

                // Reject cancellation request (clear CancelledReason)
                await _orderService.RejectCancellationRequestAsync(orderId, reason);

                // Get updated order
                var updatedOrder = await _orderService.GetOrderDetailsAsync(orderId);

                // Broadcast real-time update via SignalR
                await BroadcastOrderUpdateAsync(
                    updatedOrder,
                    updatedOrder.Status.ToString(),
                    updatedOrder.Status == OrderStatus.PENDING ? "Chờ xử lý" : "Đã xác nhận"
                );

                // Send realtime notification to customer via HTTP
                await SendOrderUpdateNotificationAsync(
                    updatedOrder,
                    updatedOrder.Status.ToString(),
                    updatedOrder.Status == OrderStatus.PENDING ? "Chờ xử lý" : "Đã xác nhận"
                );

                TempData["Success"] = "Đã từ chối yêu cầu hủy đơn hàng";
                return RedirectToPage("./Details", new { id = orderId });
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToPage("./Details", new { id = orderId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToPage("./Details", new { id = orderId });
            }
        }

        private async Task SendOrderUpdateNotificationAsync(
            OrderResponseDTO order,
            string status,
            string statusText
        )
        {
            try
            {
                // Get MVC base URL from configuration or use default
                var mvcBaseUrl = _configuration["MVCBaseUrl"] ?? "http://localhost:5045";

                var notification = new
                {
                    orderId = order.Id,
                    orderNumber = order.OrderNumber,
                    userId = order.UserId,
                    status = status,
                    statusText = statusText,
                    message = $"Đơn hàng #{order.OrderNumber} đã được cập nhật trạng thái thành: {statusText}",
                };

                _logger.LogInformation(
                    "Sending order update notification to MVC: OrderId={OrderId}, Status={Status}, StatusText={StatusText}, UserId={UserId}",
                    order.Id,
                    status,
                    statusText,
                    order.UserId
                );

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10); // Set timeout

                var json = JsonSerializer.Serialize(notification);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(
                    $"{mvcBaseUrl}/api/OrderNotification/order-updated",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation(
                        "Order update notification sent successfully for OrderId={OrderId}. Response: {Response}",
                        order.Id,
                        responseContent
                    );
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Failed to send order update notification. StatusCode={StatusCode}, Response: {Response}",
                        response.StatusCode,
                        errorContent
                    );
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail the operation
                _logger.LogError(
                    ex,
                    "Error sending order update notification for OrderId={OrderId}",
                    order.Id
                );
            }
        }

        /// <summary>
        /// Broadcast order update via SignalR to both admin and customer views
        /// </summary>
        private async Task BroadcastOrderUpdateAsync(
            OrderResponseDTO order,
            string status,
            string statusText
        )
        {
            try
            {
                var orderGroup = $"order-{order.Id}";
                var userGroup = $"user-{order.UserId}";

                // Broadcast to order-specific group (anyone viewing this order)
                await _orderHub
                    .Clients.Group(orderGroup)
                    .SendAsync(
                        "OrderUpdated",
                        new
                        {
                            orderId = order.Id,
                            orderNumber = order.OrderNumber,
                            userId = order.UserId,
                            status = status,
                            statusText = statusText,
                            message = $"Đơn hàng #{order.OrderNumber} đã được cập nhật trạng thái thành: {statusText}",
                            timestamp = DateTime.UtcNow,
                        }
                    );

                // Broadcast to user-specific group (customer)
                await _orderHub
                    .Clients.Group(userGroup)
                    .SendAsync(
                        "OrderUpdated",
                        new
                        {
                            orderId = order.Id,
                            orderNumber = order.OrderNumber,
                            userId = order.UserId,
                            status = status,
                            statusText = statusText,
                            message = $"Đơn hàng #{order.OrderNumber} đã được cập nhật trạng thái thành: {statusText}",
                            timestamp = DateTime.UtcNow,
                        }
                    );

                _logger.LogInformation(
                    "Broadcasted order update via SignalR for OrderId={OrderId} to groups: {OrderGroup}, {UserGroup}",
                    order.Id,
                    orderGroup,
                    userGroup
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error broadcasting order update via SignalR for OrderId={OrderId}",
                    order.Id
                );
            }
        }

        private string GetStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.PENDING => "Chờ xử lý",
                OrderStatus.CONFIRMED => "Đã xác nhận",
                OrderStatus.PROCESSING => "Đang xử lý",
                OrderStatus.SHIPPED => "Đang giao",
                OrderStatus.DELIVERED => "Đã giao",
                OrderStatus.CANCELLED => "Đã hủy",
                _ => status.ToString(),
            };
        }
    }
}
