using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Auth
{
    public class SyncSessionModel : PageModel
    {
        public IActionResult OnGet(int userId, string email, string userName, string roles)
        {
            // Set session data
            HttpContext.Session.SetInt32("UserId", userId);
            HttpContext.Session.SetString("UserEmail", email);
            HttpContext.Session.SetString("UserName", userName);
            HttpContext.Session.SetString("IsAuthenticated", "true");
            HttpContext.Session.SetString("UserRoles", roles);

            return RedirectToPage("/Index");
        }
    }
}

