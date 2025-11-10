using BLL.Service.Banner.Interface;
using DAL.DTOs.Banner;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using BLL.Service.Cloudinary.Interface;

namespace GreenTech.Pages.Banners
{
    [AdminAuthorize]
    public class EditModel : PageModel
    {
        private readonly IBannerService _bannerService;
        private readonly IFileStorageService _fileStorageService;

        public EditModel(IBannerService bannerService, IFileStorageService fileStorageService)
        {
            _bannerService = bannerService;
            _fileStorageService = fileStorageService;
        }

        [BindProperty]
        public UpdateBannerDTO Banner { get; set; }

        public SelectList Positions { get; set; } = new SelectList(
            new[]
            {
                new { Value = "HEADER", Text = "Header" },
                new { Value = "HOME_SLIDER", Text = "Home Slider" },
                new { Value = "SIDEBAR", Text = "Sidebar" },
                new { Value = "FOOTER", Text = "Footer" },
                new { Value = "POPUP", Text = "Popup" },
            },
            "Value", "Text"
        );

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var banner = await _bannerService.GetByIdAsync(id);
            if (banner == null) return NotFound();

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
            // if (!ModelState.IsValid)
            //     return Page();

            var existing = await _bannerService.GetByIdAsync(id);
            if (existing == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy banner cần cập nhật.");
                return Page();
            }

            var file = Request.Form.Files["ImageFile"];
            if (file != null && file.Length > 0)
            {
                // Upload ảnh mới
                var imageUrl = await _fileStorageService.SaveFileAsync(file, "banners");
                Banner.ImageUrl = imageUrl;
            }
            else
            {
                // Giữ nguyên ảnh cũ nếu không chọn ảnh mới
                Banner.ImageUrl = existing.ImageUrl;
            }
     // if (!ModelState.IsValid)
            //     return Page();
            try
            {
                await _bannerService.UpdateAsync(id, Banner);
                return RedirectToPage("./Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật banner.");
                return Page();
            }
        }
    }
}
