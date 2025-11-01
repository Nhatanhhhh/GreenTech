using BLL.Service.CouponTemplate.Interface;
using DAL.DTOs.CouponTemplate;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.CouponTemplates
{
    [AdminAuthorize]
    public class DeleteModel : PageModel
    {
        private readonly ICouponTemplateService _couponTemplateService;

        public DeleteModel(ICouponTemplateService couponTemplateService)
        {
            _couponTemplateService = couponTemplateService;
        }

        public CouponTemplateDTO Template { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Template = await _couponTemplateService.GetByIdAsync(Id);
            if (Template == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            // Ensure session is available before checking
            if (!HttpContext.Session.IsAvailable)
            {
                await HttpContext.Session.LoadAsync();
            }

            // Check authentication before processing
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            var userRoles = HttpContext.Session.GetString("UserRoles");

            // If session is expired, reload the page with error instead of redirecting to login
            if (string.IsNullOrEmpty(isAuthenticated) || isAuthenticated != "true")
            {
                Template = await _couponTemplateService.GetByIdAsync(Id);
                ModelState.AddModelError(
                    string.Empty,
                    "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
                );
                return Page();
            }

            if (string.IsNullOrEmpty(userRoles) || !userRoles.Contains("ROLE_ADMIN"))
            {
                Template = await _couponTemplateService.GetByIdAsync(Id);
                ModelState.AddModelError(
                    string.Empty,
                    "Bạn không có quyền thực hiện thao tác này."
                );
                return Page();
            }

            // Get id from Template.Id (bound from form) or use Id from route
            var idToDelete = Template?.Id > 0 ? Template.Id : Id;
            if (idToDelete == 0)
            {
                ModelState.AddModelError("", "ID template không hợp lệ.");
                Template = await _couponTemplateService.GetByIdAsync(Id);
                return Page();
            }

            try
            {
                await _couponTemplateService.DeleteAsync(idToDelete);
                TempData["SuccessMessage"] = "Xóa template thành công!";
                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                Template = await _couponTemplateService.GetByIdAsync(Id);
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi xóa template.");
                Template = await _couponTemplateService.GetByIdAsync(Id);
                return Page();
            }
        }
    }
}
