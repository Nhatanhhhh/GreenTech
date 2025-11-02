using System.IO;
using System.Text;
using System.Text.Json;
using BLL.Service.CouponTemplate.Interface;
using DAL.DTOs.CouponTemplate;
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

        public async Task<IActionResult> OnPostCreateAsync()
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
                ModelState.AddModelError(
                    string.Empty,
                    "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
                );
                DiscountTypes = new SelectList(
                    new List<SelectListItem>
                    {
                        new SelectListItem { Value = "PERCENT", Text = "Phần trăm (%)" },
                        new SelectListItem { Value = "FIXED_AMOUNT", Text = "Số tiền cố định" },
                    },
                    "Value",
                    "Text"
                );
                return Page();
            }

            if (string.IsNullOrEmpty(userRoles) || !userRoles.Contains("ROLE_ADMIN"))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Bạn không có quyền thực hiện thao tác này."
                );
                DiscountTypes = new SelectList(
                    new List<SelectListItem>
                    {
                        new SelectListItem { Value = "PERCENT", Text = "Phần trăm (%)" },
                        new SelectListItem { Value = "FIXED_AMOUNT", Text = "Số tiền cố định" },
                    },
                    "Value",
                    "Text"
                );
                return Page();
            }

            // Re-populate DiscountTypes dropdown before validation check
            DiscountTypes = new SelectList(
                new List<SelectListItem>
                {
                    new SelectListItem { Value = "PERCENT", Text = "Phần trăm (%)" },
                    new SelectListItem { Value = "FIXED_AMOUNT", Text = "Số tiền cố định" },
                },
                "Value",
                "Text"
            );

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _couponTemplateService.CreateAsync(Template);
                TempData["SuccessMessage"] = "Tạo template thành công!";
                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi tạo template.");
                return Page();
            }
        }

        /// <summary>
        /// AJAX endpoint để validate DiscountValue từ server
        /// </summary>
        public async Task<IActionResult> OnPostValidateDiscountValue()
        {
            try
            {
                Request.EnableBuffering();
                string requestBody;
                Request.Body.Position = 0;
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                Request.Body.Position = 0;

                if (string.IsNullOrEmpty(requestBody))
                {
                    return new JsonResult(new { isValid = false, message = "Thiếu thông tin" });
                }

                var request = JsonSerializer.Deserialize<ValidateDiscountValueRequest>(
                    requestBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (request == null || string.IsNullOrEmpty(request.DiscountType))
                {
                    return new JsonResult(new { isValid = false, message = "Thiếu thông tin" });
                }

                if (!request.DiscountValue.HasValue)
                {
                    return new JsonResult(
                        new { isValid = false, message = "Giá trị không hợp lệ" }
                    );
                }

                var discountValue = request.DiscountValue.Value;
                var discountType = request.DiscountType;
                var isValid = true;
                var message = "";

                if (discountType == "PERCENT")
                {
                    if (discountValue < 0 || discountValue > 100)
                    {
                        isValid = false;
                        message = "Giảm giá theo phần trăm phải từ 0% đến 100%";
                    }
                }
                else if (discountType == "FIXED_AMOUNT")
                {
                    if (discountValue < 0 || discountValue > 500000)
                    {
                        isValid = false;
                        message = "Giảm giá theo số tiền cố định phải từ 0 đến 500,000 VND";
                    }
                }

                return new JsonResult(
                    new
                    {
                        isValid = isValid,
                        message = message,
                        discountType = discountType,
                        discountValue = discountValue,
                    }
                );
            }
            catch (Exception ex)
            {
                return new JsonResult(new { isValid = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
