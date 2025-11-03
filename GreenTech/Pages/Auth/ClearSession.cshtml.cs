using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Auth
{
    /// <summary>
    /// Endpoint to clear session (for logout synchronization from MVC app)
    /// </summary>
    public class ClearSessionModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Clear session
            HttpContext.Session.Clear();

            // Delete session cookie with proper options to ensure it's deleted
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(-1), // Set to past to delete
            };
            Response.Cookies.Delete(".GreenTech.Session", cookieOptions);
            Response.Cookies.Append(".GreenTech.Session", "", cookieOptions); // Overwrite with empty

            Console.WriteLine("[ClearSession] Session cleared in RazorPages app");

            // Return success response
            return new JsonResult(new { success = true, message = "Session cleared" });
        }
    }
}
