using BLL.Service.Banner.Interface;
using DAL.DTOs.Banner;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Banners
{
    [AdminAuthorize]
    public class DeleteModel : PageModel
    {
        private readonly IBannerService _bannerService;

        public DeleteModel(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }

        public BannerDTO Banner { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Banner = await _bannerService.GetByIdAsync(id);
            if (Banner == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                var result = await _bannerService.DeleteAsync(id);
                if (!result)
                {
                    ModelState.AddModelError(string.Empty, "Không tìm thấy banner cần xóa.");
                    return Page();
                }

                return RedirectToPage("./Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi xóa banner.");
                Banner = await _bannerService.GetByIdAsync(id);
                return Page();
            }
        }
    }
}
