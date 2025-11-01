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
                return Redirect("https://localhost:7135/Auth/Login");
            }

            return RedirectToPage("/Index");
        }
    }
}
