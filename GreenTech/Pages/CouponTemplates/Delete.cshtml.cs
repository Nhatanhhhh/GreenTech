using BLL.Service.Interface;
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

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Template = await _couponTemplateService.GetByIdAsync(id);
            if (Template == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                await _couponTemplateService.DeleteAsync(id);
                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                Template = await _couponTemplateService.GetByIdAsync(id);
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi xóa template.");
                Template = await _couponTemplateService.GetByIdAsync(id);
                return Page();
            }
        }
    }
}

