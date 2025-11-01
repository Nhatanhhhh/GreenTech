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

        public async Task<IActionResult> OnPostUpdateAsync()
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
            // This prevents accidental logout when user is still on the page
            if (string.IsNullOrEmpty(isAuthenticated) || isAuthenticated != "true")
            {
                // Load template data back for display
                if (Template?.Id != null && Template.Id > 0)
                {
                    Template = await _couponTemplateService.GetByIdAsync(Template.Id);
                }

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
                // Load template data back for display
                if (Template?.Id != null && Template.Id > 0)
                {
                    Template = await _couponTemplateService.GetByIdAsync(Template.Id);
                }

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

            // Get id from Template.Id (bound from form)
            if (Template?.Id == null || Template.Id == 0)
            {
                ModelState.AddModelError("", "ID template không hợp lệ.");
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

            var id = Template.Id;

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
                // Log ModelState errors for debugging
                foreach (var error in ModelState)
                {
                    foreach (var errorMessage in error.Value.Errors)
                    {
                        Console.WriteLine(
                            $"Field: {error.Key}, Error: {errorMessage.ErrorMessage}"
                        );
                    }
                }

                Template = await _couponTemplateService.GetByIdAsync(id);
                return Page();
            }

            try
            {
                await _couponTemplateService.UpdateAsync(id, Template);
                TempData["SuccessMessage"] = "Cập nhật template thành công!";
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
                ModelState.AddModelError(
                    string.Empty,
                    $"Đã xảy ra lỗi khi cập nhật template: {ex.Message}"
                );
                Template = await _couponTemplateService.GetByIdAsync(id);
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

    /// <summary>
    /// Request model for AJAX validation
    /// </summary>
    public class ValidateDiscountValueRequest
    {
        public string DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
    }
}
