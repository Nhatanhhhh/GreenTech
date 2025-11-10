using BLL.Service.Banner.Interface;
using DAL.DTOs.Banner;
using GreenTech.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using BLL.Service.Cloudinary.Interface;

namespace GreenTech.Pages.Banners
{
    [AdminAuthorize]
    public class CreateModel : PageModel
    {
        private readonly IBannerService _bannerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFileStorageService _fileStorageService;

        public CreateModel(IBannerService bannerService
        , IHttpContextAccessor httpContextAccessor
        , IFileStorageService fileStorageService
        )
        {
            _bannerService = bannerService;
            _httpContextAccessor = httpContextAccessor;
            _fileStorageService = fileStorageService;
        }

        [BindProperty]
        public CreateBannerDTO Banner { get; set; } = new CreateBannerDTO();
        public SelectList Positions { get; set; } =
            new SelectList(
                new List<SelectListItem>
                {
                    new SelectListItem { Value = "HEADER", Text = "Header" },
                    new SelectListItem { Value = "HOME_SLIDER", Text = "Home Slider" },
                    new SelectListItem { Value = "SIDEBAR", Text = "Sidebar" },
                    new SelectListItem { Value = "FOOTER", Text = "Footer" },
                    new SelectListItem { Value = "POPUP", Text = "Popup" },
                },
                "Value",
                "Text"
            );

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            var file = Request.Form.Files["ImageFile"];

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("Banner.ImageUrl", "Vui lòng chọn ảnh để tải lên.");
                return Page();
            }

            var imageUrl = await _fileStorageService.SaveFileAsync(file, "banners");

            Banner.ImageUrl = imageUrl;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId") ?? 0;
                if (userId == 0)
                {
                    ModelState.AddModelError("", "Không tìm thấy thông tin người dùng.");
                    return Page();
                }

                await _bannerService.CreateAsync(Banner, userId);
                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi tạo banner.");
                return Page();
            }
        }
    }
}
