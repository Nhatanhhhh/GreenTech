using GreenTech.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Auth
{
    public class SyncSessionModel : PageModel
    {
        public IActionResult OnGet(int userId, string email, string userName, string roles)
        {
            // Clear any existing session to avoid conflicts
            // This ensures we start with a fresh session
            HttpContext.Session.Clear();

            // Force session to be loaded and available
            // This creates a new session in the store
            _ = HttpContext.Session.Id;

            // Set session data after ensuring session is available
            HttpContext.Session.SetInt32("UserId", userId);
            HttpContext.Session.SetString("UserEmail", email);
            HttpContext.Session.SetString("UserName", userName);
            HttpContext.Session.SetString("IsAuthenticated", "true");
            HttpContext.Session.SetString("UserRoles", roles);

            // Verify session data was set correctly by reading it back
            // This ensures the session is committed to the store
            var verifyAuth = HttpContext.Session.GetString("IsAuthenticated");
            var verifyRoles = HttpContext.Session.GetString("UserRoles");

            // If verification fails, something went wrong
            if (string.IsNullOrEmpty(verifyAuth) || verifyAuth != "true")
            {
                // Session was not saved correctly
                var loginUrl = UrlHelper.GetLoginUrl(HttpContext);
                return Redirect(loginUrl);
            }

            // Redirect based on role
            // Staff redirects to Orders page, Admin redirects to Dashboard
            var isStaff =
                !string.IsNullOrEmpty(roles)
                && roles.Contains("ROLE_STAFF")
                && !roles.Contains("ROLE_ADMIN");

            if (isStaff)
            {
                return RedirectToPage("/Orders/Index");
            }

            return RedirectToPage("/Index");
        }
    }
}
