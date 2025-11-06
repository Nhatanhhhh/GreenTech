using BLL.Service.Auth.Interface;
using GreenTech.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IHttpClientFactory _httpClientFactory;

        public LogoutModel(IAuthService authService, IHttpClientFactory httpClientFactory)
        {
            _authService = authService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _authService.LogoutAsync();

            // Clear local session cookie properly
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(-1),
            };
            Response.Cookies.Delete(".GreenTech.Session", cookieOptions);
            Response.Cookies.Append(".GreenTech.Session", "", cookieOptions);

            // Notify MVC app to clear session (logout synchronization)
            try
            {
                var mvcBaseUrl = UrlHelper.GetMvcBaseUrl(HttpContext);
                var mvcAppUrl = $"{mvcBaseUrl}/Auth/ClearSession";
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5); // Short timeout

                // Try to send request and wait briefly, but don't block on failure
                var sendTask = httpClient.GetAsync(mvcAppUrl);
                var delayTask = Task.Delay(100); // Small delay to allow request to start
                await Task.WhenAny(sendTask, delayTask); // Wait for either to complete

                if (sendTask.IsCompleted)
                {
                    if (sendTask.IsFaulted || sendTask.IsCanceled)
                    {
                        Console.WriteLine(
                            $"[Logout Sync] Failed to notify MVC app: {sendTask.Exception?.GetBaseException()?.Message}"
                        );
                    }
                    else
                    {
                        Console.WriteLine(
                            "[Logout Sync] Successfully notified MVC app to clear session"
                        );
                    }
                }
                else
                {
                    // Request is still pending, continue with logout anyway (fire and forget)
                    _ = sendTask.ContinueWith(task =>
                    {
                        if (task.IsFaulted || task.IsCanceled)
                        {
                            Console.WriteLine(
                                $"[Logout Sync] Failed to notify MVC app: {task.Exception?.GetBaseException()?.Message}"
                            );
                        }
                        else
                        {
                            Console.WriteLine(
                                "[Logout Sync] Successfully notified MVC app to clear session"
                            );
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw - logout should succeed even if sync fails
                Console.WriteLine($"[Logout Sync] Error: {ex.Message}");
            }

            var loginUrl = UrlHelper.GetLoginUrl(HttpContext);
            return Redirect(loginUrl);
        }
    }
}
