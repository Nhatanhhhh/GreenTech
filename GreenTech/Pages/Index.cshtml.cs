using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages
{
    [AdminAuthorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Check if user is Staff - redirect to Orders page
            var userRoles = HttpContext.Session.GetString("UserRoles");
            var isStaff =
                !string.IsNullOrEmpty(userRoles)
                && userRoles.Contains("ROLE_STAFF")
                && !userRoles.Contains("ROLE_ADMIN");

            if (isStaff)
            {
                // Staff cannot access Dashboard, redirect to Orders
                HttpContext.Session.SetString(
                    "ErrorMessage",
                    "Bạn không có quyền truy cập trang Dashboard. Vui lòng sử dụng trang Quản lý đơn hàng."
                );
                return RedirectToPage("/Orders/Index");
            }

            // Check for error message from session (set by authorization filters)
            var errorMessage = HttpContext.Session.GetString("ErrorMessage");
            if (!string.IsNullOrEmpty(errorMessage))
            {
                TempData["ErrorMessage"] = errorMessage;
                HttpContext.Session.Remove("ErrorMessage"); // Clear after displaying
            }

            return Page();
        }
    }
}
