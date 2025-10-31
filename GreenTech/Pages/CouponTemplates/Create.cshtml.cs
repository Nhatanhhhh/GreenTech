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
    public class CreateModel : PageModel
    {
        private readonly ICouponTemplateService _couponTemplateService;

        public CreateModel(ICouponTemplateService couponTemplateService)
        {
            _couponTemplateService = couponTemplateService;
        }

        [BindProperty]
        public CreateCouponTemplateDTO Template { get; set; } = new CreateCouponTemplateDTO();

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

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _couponTemplateService.CreateAsync(Template);
                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi tạo template.");
                return Page();
            }
        }
    }
}
