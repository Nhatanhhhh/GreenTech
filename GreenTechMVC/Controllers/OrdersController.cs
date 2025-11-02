using System.Text;
using System.Text.Json;
using BLL.Service.Cart.Interface;
using BLL.Service.Order.Interface;
using BLL.Service.User.Interface;
using BLL.Service.Wallet.Interface;
using DAL.DTOs.Order;
using DAL.DTOs.User;
using DAL.Models.Enum;
using GreenTechMVC.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GreenTechMVC.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IWalletService _walletService;
        private readonly IUserService _userService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            ICartService cartService,
            IWalletService walletService,
            IUserService userService,
            IHubContext<NotificationHub> hubContext,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<OrdersController> logger
        )
        {
            _orderService = orderService;
            _cartService = cartService;
            _walletService = walletService;
            _userService = userService;
            _hubContext = hubContext;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Hiển thị danh sách đơn hàng của user
        /// </summary>
        public async Task<IActionResult> Index(OrderStatus? status = null)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var orders = status.HasValue
                ? await _orderService.GetMyOrdersAsync(userId, status)
                : await _orderService.GetMyOrdersAsync(userId);

            ViewBag.SelectedStatus = status;
            return View(orders);
        }

        /// <summary>
        /// Hiển thị chi tiết đơn hàng
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var order = await _orderService.GetOrderDetailsAsync(id, userId);
                return View(order);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Index");
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Hiển thị trang checkout (tạo đơn hàng từ giỏ hàng)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get cart
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống, không thể thanh toán";
                return RedirectToAction("Index", "Cart");
            }

            // Get user profile to pre-fill form
            var userProfile = await _userService.GetProfileAsync(userId);

            // Get user wallet balance
            var walletBalance = await _walletService.GetWalletBalanceAsync(userId);

            // Calculate total order amount
            var shippingFee = 30000m; // Default shipping fee
            var totalAmount = cart.Total + shippingFee;

            // Automatically use wallet balance (use all available balance or total amount, whichever is smaller)
            var walletAmountUsed = Math.Min(walletBalance, totalAmount);

            ViewBag.WalletBalance = walletBalance;
            ViewBag.Cart = cart;
            ViewBag.UserProfile = userProfile;

            // Build shipping address from user profile if available
            var shippingAddress = "";
            if (!string.IsNullOrEmpty(userProfile.SpecificAddress))
            {
                var addressParts = new List<string> { userProfile.SpecificAddress };
                if (!string.IsNullOrEmpty(userProfile.Ward))
                    addressParts.Add(userProfile.Ward);
                if (!string.IsNullOrEmpty(userProfile.District))
                    addressParts.Add(userProfile.District);
                if (!string.IsNullOrEmpty(userProfile.Province))
                    addressParts.Add(userProfile.Province);
                shippingAddress = string.Join(", ", addressParts);
            }

            var model = new CreateOrderDTO
            {
                UserId = userId,
                CustomerName = userProfile.FullName ?? "",
                CustomerPhone = userProfile.Phone ?? "",
                ShippingAddress = shippingAddress,
                ShippingFee = shippingFee,
                WalletAmountUsed = walletAmountUsed,
            };

            return View(model);
        }

        /// <summary>
        /// Xử lý tạo đơn hàng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderDTO model)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Clear Note value if it's empty/whitespace (since it's optional)
            if (string.IsNullOrWhiteSpace(model.Note))
            {
                model.Note = null;
            }

            // Remove any validation errors for Note since it's optional
            if (ModelState.ContainsKey("Note"))
            {
                ModelState.Remove("Note");
            }

            if (!ModelState.IsValid)
            {
                var cart = await _cartService.GetCartByUserIdAsync(userId);
                var walletBalance = await _walletService.GetWalletBalanceAsync(userId);
                var userProfile = await _userService.GetProfileAsync(userId);
                ViewBag.WalletBalance = walletBalance;
                ViewBag.Cart = cart;
                ViewBag.UserProfile = userProfile;
                return View("Checkout", model);
            }

            try
            {
                model.UserId = userId; // Ensure UserId is set correctly

                // Get current cart to calculate total
                var cart = await _cartService.GetCartByUserIdAsync(userId);
                var walletBalance = await _walletService.GetWalletBalanceAsync(userId);
                var totalAmount = cart.Total + model.ShippingFee;

                // Automatically use wallet balance (all available or total amount, whichever is smaller)
                model.WalletAmountUsed = Math.Min(walletBalance, totalAmount);

                // Update user profile if information changed
                var userProfile = await _userService.GetProfileAsync(userId);
                bool profileChanged = false;

                // Only update FullName and Phone, not SpecificAddress
                // (ShippingAddress format is different from SpecificAddress - it's a full address string)
                if (
                    userProfile.FullName != model.CustomerName
                    || userProfile.Phone != model.CustomerPhone
                )
                {
                    var updateProfileDto = new UpdateProfileDTO
                    {
                        FullName = model.CustomerName,
                        Phone = model.CustomerPhone,
                        Email = userProfile.Email, // Keep existing email
                        Province = userProfile.Province ?? "", // Keep existing or default
                        District = userProfile.District ?? "", // Keep existing or default
                        Ward = userProfile.Ward ?? "", // Keep existing or default
                        SpecificAddress = userProfile.SpecificAddress, // Keep existing specific address (not from ShippingAddress)
                    };

                    await _userService.UpdateProfileAsync(userId, updateProfileDto);
                    profileChanged = true;
                }

                var order = await _orderService.CreateOrderAsync(model);

                if (profileChanged)
                {
                    TempData["Success"] =
                        $"Đơn hàng #{order.OrderNumber} đã được tạo thành công! Thông tin cá nhân đã được cập nhật.";
                }
                else
                {
                    TempData["Success"] = $"Đơn hàng #{order.OrderNumber} đã được tạo thành công!";
                }

                // Send realtime notification to admin (Razor Pages) about new order
                await SendNewOrderNotificationToAdminAsync(order);

                return RedirectToAction("Details", new { id = order.Id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                var cart = await _cartService.GetCartByUserIdAsync(userId);
                var walletBalance = await _walletService.GetWalletBalanceAsync(userId);
                ViewBag.WalletBalance = walletBalance;
                ViewBag.Cart = cart;
                return View("Checkout", model);
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
                var cart = await _cartService.GetCartByUserIdAsync(userId);
                var walletBalance = await _walletService.GetWalletBalanceAsync(userId);
                ViewBag.WalletBalance = walletBalance;
                ViewBag.Cart = cart;
                return View("Checkout", model);
            }
        }

        /// <summary>
        /// Hủy đơn hàng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(CancelOrderDTO model)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            model.UserId = userId; // Set UserId for validation

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ";
                return RedirectToAction("Details", new { id = model.OrderId });
            }

            try
            {
                // Get order details before cancellation request
                var order = await _orderService.GetOrderDetailsAsync(model.OrderId, userId);

                var success = await _orderService.CancelOrderAsync(model);
                if (success)
                {
                    TempData["Success"] =
                        "Yêu cầu hủy đơn hàng đã được gửi. Vui lòng chờ admin xác nhận.";

                    // Send realtime notification to admin (Razor Pages)
                    await SendCancellationRequestToAdminAsync(order, model.Reason);

                    // Send realtime notification to customer (confirming request sent)
                    await SendCancellationRequestConfirmationAsync(order.Id, order.OrderNumber);
                }
                else
                {
                    TempData["Error"] = "Không thể gửi yêu cầu hủy đơn hàng";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", new { id = model.OrderId });
        }

        private int GetCurrentUserId()
        {
            // Lấy từ Session
            var userIdFromSession = HttpContext.Session.GetInt32("UserId");
            if (userIdFromSession.HasValue && userIdFromSession.Value > 0)
            {
                return userIdFromSession.Value;
            }

            // Hoặc lấy từ Claims nếu có
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userIdFromClaims))
            {
                return userIdFromClaims;
            }

            return 0;
        }

        /// <summary>
        /// Gửi realtime notification đến Admin (Razor Pages) khi có order mới được tạo
        /// </summary>
        private async Task SendNewOrderNotificationToAdminAsync(OrderResponseDTO order)
        {
            try
            {
                // Get Razor Pages base URL from configuration or use default
                var razorPagesBaseUrl =
                    _configuration["RazorPagesBaseUrl"] ?? "https://localhost:7142";

                var notification = new
                {
                    orderId = order.Id,
                    orderNumber = order.OrderNumber,
                    userId = order.UserId,
                    customerName = order.CustomerName,
                    total = order.Total,
                    status = order.Status.ToString(),
                    message = $"Đơn hàng mới #{order.OrderNumber} từ khách hàng {order.CustomerName}",
                    timestamp = DateTime.UtcNow,
                };

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5); // Quick timeout for realtime
                var json = JsonSerializer.Serialize(notification);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send to Razor Pages handler endpoint
                var response = await httpClient.PostAsync(
                    $"{razorPagesBaseUrl}/Orders/ReceiveNewOrder",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "New order notification sent to admin for OrderId={OrderId}",
                        order.Id
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to send new order notification. StatusCode={StatusCode}",
                        response.StatusCode
                    );
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail the operation
                _logger.LogError(
                    ex,
                    "Error sending new order notification for OrderId={OrderId}",
                    order.Id
                );
            }
        }

        /// <summary>
        /// Gửi realtime notification đến Admin (Razor Pages) khi customer gửi yêu cầu hủy
        /// </summary>
        private async Task SendCancellationRequestToAdminAsync(
            OrderResponseDTO order,
            string reason
        )
        {
            try
            {
                // Get Razor Pages base URL from configuration or use default
                var razorPagesBaseUrl =
                    _configuration["RazorPagesBaseUrl"] ?? "https://localhost:7142";

                var notification = new
                {
                    orderId = order.Id,
                    orderNumber = order.OrderNumber,
                    userId = order.UserId,
                    customerName = order.CustomerName,
                    reason = reason,
                    message = $"Khách hàng {order.CustomerName} đã gửi yêu cầu hủy đơn hàng #{order.OrderNumber}",
                    timestamp = DateTime.UtcNow,
                };

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5); // Quick timeout for realtime
                var json = JsonSerializer.Serialize(notification);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send to Razor Pages handler endpoint
                var response = await httpClient.PostAsync(
                    $"{razorPagesBaseUrl}/Orders/ReceiveCancellationRequest",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Cancellation request notification sent to admin for OrderId={OrderId}",
                        order.Id
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to send cancellation request notification. StatusCode={StatusCode}",
                        response.StatusCode
                    );
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail the operation
                _logger.LogError(
                    ex,
                    "Error sending cancellation request notification for OrderId={OrderId}",
                    order.Id
                );
            }
        }

        /// <summary>
        /// Gửi realtime notification đến Customer để xác nhận yêu cầu hủy đã được gửi
        /// </summary>
        private async Task SendCancellationRequestConfirmationAsync(int orderId, string orderNumber)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId > 0)
                {
                    var userGroup = $"user-{userId}";
                    var orderGroup = $"order-{orderId}";

                    await _hubContext
                        .Clients.Groups(userGroup, orderGroup)
                        .SendAsync(
                            "CancellationRequestSent",
                            new
                            {
                                orderId = orderId,
                                orderNumber = orderNumber,
                                message = $"Yêu cầu hủy đơn hàng #{orderNumber} đã được gửi. Vui lòng chờ admin xác nhận.",
                                timestamp = DateTime.UtcNow,
                            }
                        );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error sending cancellation request confirmation for OrderId={OrderId}",
                    orderId
                );
            }
        }
    }
}
