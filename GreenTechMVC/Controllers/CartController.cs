using BLL.Service.Cart.Interface;
using BLL.Service.Wallet.Interface;
using DAL.DTOs.Cart;
using GreenTechMVC.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GreenTechMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IWalletService _walletService;
        private readonly IHubContext<CartHub> _hubContext;

        public CartController(
            ICartService cartService,
            IWalletService walletService,
            IHubContext<CartHub> hubContext
        )
        {
            _cartService = cartService;
            _walletService = walletService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Hiển thị giỏ hàng của user
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Lấy user ID từ session hoặc claims
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return View(cart);
        }

        /// <summary>
        /// Kiểm tra số dư ví và quyết định hướng đi tiếp theo khi thanh toán
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ProceedToCheckout()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            var walletBalance = await _walletService.GetWalletBalanceAsync(userId);

            var total = cart.Total;
            if (walletBalance < total)
            {
                var shortfall = total - walletBalance;
                return Json(
                    new
                    {
                        success = false,
                        needTopUp = true,
                        shortfall = shortfall,
                        redirectUrl = Url.Action(
                            "TopUp",
                            "Wallet",
                            new { amount = Math.Ceiling(shortfall) }
                        ),
                    }
                );
            }

            // Đủ tiền: redirect đến trang checkout
            return Json(
                new
                {
                    success = true,
                    message = "Số dư ví đủ để thanh toán",
                    nextUrl = Url.Action("Checkout", "Orders"),
                }
            );
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(
                        new
                        {
                            success = false,
                            message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng",
                        }
                    );
                }

                var addToCartDTO = new AddToCartDTO
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                };

                var result = await _cartService.AddToCartAsync(addToCartDTO);

                await _hubContext.Clients.All.SendAsync(
                    "CartUpdated",
                    new
                    {
                        userId = userId,
                        totalItems = result.TotalItems,
                        total = result.Total,
                    }
                );

                return Json(
                    new
                    {
                        success = true,
                        message = "Đã thêm sản phẩm vào giỏ hàng thành công",
                        cartTotal = result.Total,
                        totalItems = result.TotalItems,
                    }
                );
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(
                    new
                    {
                        success = false,
                        message = "Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng",
                    }
                );
            }
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, int quantity)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                // Validate cart item belongs to current user
                if (!await _cartService.ValidateCartItemAsync(cartItemId, userId))
                {
                    return Json(
                        new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng" }
                    );
                }

                var updateCartItemDTO = new UpdateCartItemDTO
                {
                    CartItemId = cartItemId,
                    Quantity = quantity,
                };

                var result = await _cartService.UpdateCartItemAsync(updateCartItemDTO);

                var updatedItem = result.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);

                await _hubContext.Clients.All.SendAsync(
                    "CartUpdated",
                    new
                    {
                        userId = userId,
                        totalItems = result.TotalItems,
                        total = result.Total,
                        cartItemId = cartItemId,
                        itemQuantity = updatedItem?.Quantity,
                        itemSubtotal = updatedItem?.Subtotal,
                    }
                );

                return Json(
                    new
                    {
                        success = true,
                        message = "Đã cập nhật số lượng sản phẩm thành công",
                        cartTotal = result.Total,
                        totalItems = result.TotalItems,
                        cartItemId = cartItemId,
                        itemQuantity = updatedItem?.Quantity,
                        itemSubtotal = updatedItem?.Subtotal,
                    }
                );
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(
                    new { success = false, message = "Có lỗi xảy ra khi cập nhật giỏ hàng" }
                );
            }
        }

        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RemoveCartItem(int cartItemId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var result = await _cartService.RemoveCartItemAsync(cartItemId, userId);

                if (result)
                {
                    // Lấy giỏ hàng sau khi xóa để trả về thông tin cập nhật
                    var cart = await _cartService.GetCartByUserIdAsync(userId);

                    await _hubContext.Clients.All.SendAsync(
                        "CartUpdated",
                        new
                        {
                            userId = userId,
                            totalItems = cart.TotalItems,
                            total = cart.Total,
                        }
                    );
                    return Json(
                        new
                        {
                            success = true,
                            message = "Đã xóa sản phẩm khỏi giỏ hàng",
                            cartTotal = cart.Total,
                            totalItems = cart.TotalItems,
                        }
                    );
                }
                else
                {
                    return Json(
                        new { success = false, message = "Không thể xóa sản phẩm khỏi giỏ hàng" }
                    );
                }
            }
            catch (Exception)
            {
                return Json(
                    new
                    {
                        success = false,
                        message = "Có lỗi xảy ra khi xóa sản phẩm khỏi giỏ hàng",
                    }
                );
            }
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var result = await _cartService.ClearCartAsync(userId);

                if (result)
                {
                    await _hubContext.Clients.All.SendAsync(
                        "CartUpdated",
                        new
                        {
                            userId = userId,
                            totalItems = 0,
                            total = 0m,
                        }
                    );
                    return Json(
                        new
                        {
                            success = true,
                            message = "Đã xóa toàn bộ giỏ hàng",
                            cartTotal = 0,
                            totalItems = 0,
                        }
                    );
                }
                else
                {
                    return Json(new { success = false, message = "Không thể xóa giỏ hàng" });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa giỏ hàng" });
            }
        }

        /// <summary>
        /// Lấy thông tin giỏ hàng (cho AJAX calls)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCartInfo()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, message = "Chưa đăng nhập" });
                }

                var cart = await _cartService.GetCartByUserIdAsync(userId);

                return Json(
                    new
                    {
                        success = true,
                        cartTotal = cart.Total,
                        totalItems = cart.TotalItems,
                        cartItems = cart.CartItems,
                    }
                );
            }
            catch (Exception)
            {
                return Json(
                    new { success = false, message = "Có lỗi xảy ra khi lấy thông tin giỏ hàng" }
                );
            }
        }

        /// <summary>
        /// Lấy số lượng sản phẩm trong giỏ hàng (cho header)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { count = 0 });
                }

                var cart = await _cartService.GetCartByUserIdAsync(userId);
                // Count distinct products (unique product IDs), not total quantity
                var distinctCount =
                    cart?.CartItems?.Select(ci => ci.ProductId).Distinct().Count() ?? 0;
                return Json(new { count = distinctCount });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }

        /// <summary>
        /// Lấy user ID hiện tại từ session hoặc claims
        /// </summary>
        private int GetCurrentUserId()
        {
            // Lấy UserId từ Session; nếu chưa đăng nhập sẽ là null -> trả về 0
            var userIdFromSession = HttpContext.Session.GetInt32("UserId");
            if (userIdFromSession.HasValue)
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
    }
}
