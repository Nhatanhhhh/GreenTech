using BLL.Service.Banner.Interface;
using DAL.DTOs.Banner;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GreenTech.Pages.Banners
{
    [AdminAuthorize]
    public class EditModel : PageModel
    {
        private readonly IBannerService _bannerService;

        public EditModel(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }

        [BindProperty]
        public UpdateBannerDTO Banner { get; set; }

        public SelectList Positions { get; set; } =
            new SelectList(
                new List<SelectListItem>
                {
                    new SelectListItem { Value = "HEADER", Text = "Header" },
                    new SelectListItem { Value = "HOME_SLIDER", Text = "Home Slider" },
                    new SelectListItem { Value = "SIDEBAR", Text = "Sidebar" },
                    new SelectListItem { Value = "FOOTER", Text = "Footer" },
                    new SelectListItem { Value = "POPUP", Text = "Popup" },
                }
            );

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var banner = await _bannerService.GetByIdAsync(id);
            if (banner == null)
            {
                return NotFound();
            }

            Banner = new UpdateBannerDTO
            {
                Title = banner.Title,
                Description = banner.Description,
                ImageUrl = banner.ImageUrl,
                LinkUrl = banner.LinkUrl,
                Position = banner.Position,
                SortOrder = banner.SortOrder,
                IsActive = banner.IsActive,
                StartDate = banner.StartDate,
                EndDate = banner.EndDate,
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = await _bannerService.UpdateAsync(id, Banner);
                if (result == null)
                {
                    ModelState.AddModelError(string.Empty, "Không tìm thấy banner cần cập nhật.");
                    return Page();
                }

                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật banner.");
                return Page();
            }
        }
    }
}
