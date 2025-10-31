using BLL.Service.CouponTemplate.Interface;
using DAL.DTOs.CouponTemplate;
using DAL.Models.Enum;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GreenTech.Pages.CouponTemplates
{
    [AdminAuthorize]
    public class EditModel : PageModel
    {
        private readonly ICouponTemplateService _couponTemplateService;

        public EditModel(ICouponTemplateService couponTemplateService)
        {
            _couponTemplateService = couponTemplateService;
        }

        [BindProperty]
        public CouponTemplateDTO Template { get; set; }

        public SelectList DiscountTypes { get; set; } =
            new SelectList(
                new List<SelectListItem>
                {
                    new SelectListItem { Value = "PERCENT", Text = "Phần trăm (%)" },
                    new SelectListItem { Value = "FIXED_AMOUNT", Text = "Số tiền cố định" },
                },
                "Value",
                "Text"
            );

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
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _couponTemplateService.UpdateAsync(id, Template);
                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật template.");
                return Page();
            }
        }
    }
}
