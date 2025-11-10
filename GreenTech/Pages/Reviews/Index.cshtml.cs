using BLL.Service.Review.Interface;
using BLL.Service.ReviewReply.Interface;
using DAL.DTOs.ReviewReply;
using DAL.Models;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Reviews
{
    [AdminAuthorize]
    public class IndexModel : PageModel
    {
        private readonly IReviewService _reviewService;
        private readonly IReviewReplyService _reviewReplyService;

        public IndexModel(IReviewService reviewService, IReviewReplyService reviewReplyService)
        {
            _reviewService = reviewService;
            _reviewReplyService = reviewReplyService;
        }

        public IEnumerable<Review> Reviews { get; set; } = new List<Review>();

        public async Task OnGetAsync()
        {
            Reviews = await _reviewService.GetAllReviewsAsync();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            await _reviewService.ToggleReviewStatusAsync(id);
            return RedirectToPage(); // Refresh UI
        }

        public async Task<IActionResult> OnPostCreateReplyAsync(int reviewId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Nội dung phản hồi không được để trống.";
                return RedirectToPage();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue || userId.Value == 0)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để phản hồi.";
                return RedirectToPage();
            }

            try
            {
                var dto = new CreateReviewReplyDTO
                {
                    ReviewId = reviewId,
                    UserId = userId.Value,
                    Content = content.Trim(),
                };

                await _reviewReplyService.CreateReplyAsync(dto);
                TempData["SuccessMessage"] = "Phản hồi đã được gửi thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
