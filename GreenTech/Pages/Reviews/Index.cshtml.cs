using BLL.Service.Review.Interface;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DAL.Models;

namespace GreenTech.Pages.Reviews
{
    [AdminAuthorize]
    public class IndexModel : PageModel
    {
        private readonly IReviewService _reviewService;

        public IndexModel(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        public IEnumerable<Review> Reviews { get; set; } = new List<Review>();

        public async Task OnGetAsync()
        {
            Reviews = await _reviewService.GetAllReviewsAsync();
        }
    }
}
