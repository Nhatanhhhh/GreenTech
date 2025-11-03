using System.Linq;
using BLL.Service.Order.Interface;
using BLL.Service.Review.Interface;
using DAL.DTOs.Review;
using DAL.Models.Enum;
using Microsoft.AspNetCore.Mvc;

namespace GreenTechMVC.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IOrderService _orderService;

        public ReviewsController(IReviewService reviewService, IOrderService orderService)
        {
            _reviewService = reviewService;
            _orderService = orderService;
        }

        private int GetCurrentUserId()
        {
            var userIdFromSession = HttpContext.Session.GetInt32("UserId");
            return userIdFromSession ?? 0;
        }

        /// <summary>
        /// Tạo review cho order item (chỉ khi order đã DELIVERED)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewDTO dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            // Kiểm tra order item có tồn tại và thuộc về user không
            try
            {
                // Kiểm tra đã có review chưa
                var existingReview = await _reviewService.CheckReviewExistsForOrderItemAsync(
                    dto.OrderItemId
                );
                if (existingReview)
                {
                    return Json(
                        new { success = false, message = "Bạn đã đánh giá sản phẩm này rồi." }
                    );
                }

                // Tìm order chứa order item này bằng cách lấy tất cả orders đã DELIVERED của user
                var userOrders = await _orderService.GetMyOrdersAsync(
                    userId,
                    OrderStatus.DELIVERED
                );
                var orderWithItem = userOrders.FirstOrDefault(o =>
                    o.OrderItems.Any(oi =>
                        oi.Id == dto.OrderItemId && oi.ProductId == dto.ProductId
                    )
                );

                if (orderWithItem == null)
                {
                    return Json(
                        new
                        {
                            success = false,
                            message = "Không tìm thấy sản phẩm trong đơn hàng đã giao của bạn.",
                        }
                    );
                }

                // Set userId từ session
                dto.UserId = userId;

                var review = await _reviewService.CreateReviewAsync(dto);
                return Json(
                    new
                    {
                        success = true,
                        message = "Đánh giá thành công!",
                        reviewId = review.Id,
                    }
                );
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã có lỗi xảy ra: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách reviews theo product ID (AJAX với pagination)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetReviews(int productId, int page = 1, int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            var result = await _reviewService.GetReviewsByProductIdAsync(
                productId,
                page,
                pageSize,
                userId > 0 ? userId : null
            );

            return Json(result);
        }

        /// <summary>
        /// Lấy thống kê reviews theo product ID
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStatistics(int productId)
        {
            var stats = await _reviewService.GetReviewStatisticsByProductIdAsync(productId);
            return Json(stats);
        }

        /// <summary>
        /// Kiểm tra review đã tồn tại cho order item
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckReviewExists(int orderItemId)
        {
            var exists = await _reviewService.CheckReviewExistsForOrderItemAsync(orderItemId);
            return Json(new { exists });
        }
    }
}
