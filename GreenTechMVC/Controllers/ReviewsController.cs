using System.IO;
using System.Linq;
using BLL.Service.Cloudinary.Interface;
using BLL.Service.Order.Interface;
using BLL.Service.Review.Interface;
using DAL.DTOs.Review;
using DAL.Models.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GreenTechMVC.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IOrderService _orderService;
        private readonly IFileStorageService _fileStorageService;

        public ReviewsController(
            IReviewService reviewService,
            IOrderService orderService,
            IFileStorageService fileStorageService
        )
        {
            _reviewService = reviewService;
            _orderService = orderService;
            _fileStorageService = fileStorageService;
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
        public async Task<IActionResult> Create(
            [FromForm] CreateReviewDTO dto,
            [FromForm] List<IFormFile>? mediaFiles
        )
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            // Nếu model binding không hoạt động, đọc trực tiếp từ Request.Form
            if (dto.OrderItemId == 0 || dto.ProductId == 0 || dto.Rating == 0)
            {
                // Fallback: đọc từ Request.Form
                if (int.TryParse(Request.Form["OrderItemId"].ToString(), out int orderItemId))
                    dto.OrderItemId = orderItemId;
                if (int.TryParse(Request.Form["ProductId"].ToString(), out int productId))
                    dto.ProductId = productId;
                if (int.TryParse(Request.Form["Rating"].ToString(), out int rating))
                    dto.Rating = rating;
            }

            // Đọc Content từ Request.Form nếu null hoặc empty
            if (string.IsNullOrEmpty(dto.Content))
            {
                var contentFromForm = Request.Form["Content"].ToString();
                if (!string.IsNullOrEmpty(contentFromForm))
                {
                    dto.Content = contentFromForm;
                }
            }

            // Đọc IsAnonymous từ Request.Form nếu chưa set
            var isAnonymousValue = Request.Form["IsAnonymous"].ToString();
            if (!string.IsNullOrEmpty(isAnonymousValue))
            {
                dto.IsAnonymous = isAnonymousValue.ToLower() == "true" || isAnonymousValue == "on";
            }

            // Debug: Log dữ liệu nhận được
            Console.WriteLine(
                $"[Review Create] Received DTO - OrderItemId: {dto.OrderItemId}, ProductId: {dto.ProductId}, Rating: {dto.Rating}, Content: '{dto.Content}', IsAnonymous: {dto.IsAnonymous}"
            );
            Console.WriteLine($"[Review Create] MediaFiles count: {mediaFiles?.Count ?? 0}");

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

                // Upload media files nếu có
                var mediaUrls = new List<string>();
                if (mediaFiles != null && mediaFiles.Count > 0)
                {
                    // Giới hạn tối đa 5 ảnh
                    var filesToUpload = mediaFiles.Take(5).ToList();

                    foreach (var file in filesToUpload)
                    {
                        if (file != null && file.Length > 0)
                        {
                            // Kiểm tra định dạng file (chỉ cho phép ảnh)
                            var allowedExtensions = new[]
                            {
                                ".jpg",
                                ".jpeg",
                                ".png",
                                ".gif",
                                ".webp",
                            };
                            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                            if (!allowedExtensions.Contains(extension))
                            {
                                continue; // Bỏ qua file không hợp lệ
                            }

                            // Kiểm tra kích thước file (tối đa 10MB)
                            if (file.Length > 10 * 1024 * 1024)
                            {
                                continue; // Bỏ qua file quá lớn
                            }

                            try
                            {
                                var mediaUrl = await _fileStorageService.SaveFileAsync(
                                    file,
                                    "reviews"
                                );
                                mediaUrls.Add(mediaUrl);
                            }
                            catch (Exception ex)
                            {
                                // Log lỗi nhưng không dừng quá trình tạo review
                                Console.WriteLine($"Error uploading media file: {ex.Message}");
                            }
                        }
                    }
                }

                // Set userId từ session
                dto.UserId = userId;

                // Set media URLs (nếu có, nếu không thì set empty string để tránh NULL)
                if (mediaUrls.Count > 0)
                {
                    dto.MediaUrls = string.Join(",", mediaUrls);
                }
                else
                {
                    dto.MediaUrls = string.Empty; // Set empty string thay vì null
                }

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
            try
            {
                Console.WriteLine(
                    $"[ReviewsController.GetReviews] ====== GetReviews Called ======"
                );
                Console.WriteLine(
                    $"[ReviewsController.GetReviews] productId={productId}, page={page}, pageSize={pageSize}"
                );

                if (productId <= 0)
                {
                    Console.WriteLine(
                        "[ReviewsController.GetReviews] Invalid productId, returning empty"
                    );
                    return Json(
                        new
                        {
                            Reviews = new List<object>(),
                            TotalCount = 0,
                            PageNumber = page,
                            PageSize = pageSize,
                            TotalPages = 0,
                            HasPreviousPage = false,
                            HasNextPage = false,
                        }
                    );
                }

                var userId = GetCurrentUserId();
                Console.WriteLine(
                    $"[ReviewsController.GetReviews] Calling service with productId={productId}, page={page}, pageSize={pageSize}, userId={userId}"
                );

                var result = await _reviewService.GetReviewsByProductIdAsync(
                    productId,
                    page,
                    pageSize,
                    userId > 0 ? userId : null
                );

                Console.WriteLine(
                    $"[ReviewsController.GetReviews] Service returned - TotalCount: {result?.TotalCount ?? 0}, Reviews count: {result?.Reviews?.Count ?? 0}"
                );

                // Trả về trực tiếp result từ service (đã có đầy đủ thông tin từ ReviewPaginationDTO)
                if (result == null)
                {
                    Console.WriteLine(
                        "[ReviewsController.GetReviews] Result is null, returning empty"
                    );
                    return Json(
                        new
                        {
                            Reviews = new List<object>(),
                            TotalCount = 0,
                            PageNumber = page,
                            PageSize = pageSize,
                            TotalPages = 0,
                            HasPreviousPage = false,
                            HasNextPage = false,
                        }
                    );
                }

                // Trả về trực tiếp DTO từ service
                return Json(result);
            }
            catch (Exception ex)
            {
                // Log error chi tiết
                Console.WriteLine($"[ReviewsController.GetReviews] ====== ERROR ======");
                Console.WriteLine(
                    $"[ReviewsController.GetReviews] Error Type: {ex.GetType().Name}"
                );
                Console.WriteLine($"[ReviewsController.GetReviews] Error Message: {ex.Message}");
                Console.WriteLine($"[ReviewsController.GetReviews] StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine(
                        $"[ReviewsController.GetReviews] InnerException: {ex.InnerException.Message}"
                    );
                }

                // Trả về empty result thay vì throw exception
                return Json(
                    new
                    {
                        Reviews = new List<object>(),
                        TotalCount = 0,
                        PageNumber = page,
                        PageSize = pageSize,
                        TotalPages = 0,
                        HasPreviousPage = false,
                        HasNextPage = false,
                        Error = ex.Message,
                    }
                );
            }
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
