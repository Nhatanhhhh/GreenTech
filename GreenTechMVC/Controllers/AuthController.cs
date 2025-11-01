using BLL.Service.Auth.Interface;
using DAL.DTOs.User;
using DAL.Models.Enum;
using Microsoft.AspNetCore.Mvc;

[Route("Auth")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(
        IAuthService authService,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory
    )
    {
        _authService = authService;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    [Route("Login")]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [Route("Login")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDTO model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            try
            {
                var userResponse = await _authService.LoginAsync(model);

                if (userResponse.Roles.Contains(RoleName.ROLE_ADMIN.ToString()))
                {
                    // Get user ID from session
                    var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                    var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";
                    var userName = HttpContext.Session.GetString("UserName") ?? "";
                    var userRoles = HttpContext.Session.GetString("UserRoles") ?? "";

                    // Build admin app URL with session data
                    var adminAppUrl = "https://localhost:7142/Auth/SyncSession";
                    var redirectUrl =
                        $"{adminAppUrl}?userId={userId}&email={Uri.EscapeDataString(userEmail)}&userName={Uri.EscapeDataString(userName)}&roles={Uri.EscapeDataString(userRoles)}";

                    return Redirect(redirectUrl);
                }
                else
                {
                    return RedirectToLocal(returnUrl);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã có lỗi xảy ra khi đăng nhập.");
                Console.WriteLine($"[Error] Login failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        return View(model);
    }

    [HttpGet("Register")]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new RegisterDTO());
    }

    [HttpPost("Register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDTO model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        ModelState.Remove(nameof(model.Avatar));
        // AgreeToTerms validation is handled on frontend only

        if (ModelState.IsValid)
        {
            try
            {
                await _authService.RegisterAsync(model);

                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login", "Auth", new { returnUrl });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã có lỗi xảy ra trong quá trình đăng ký.");
                Console.WriteLine($"[Error] Register failed: {ex.Message}");
            }
        }

        return View(model);
    }

    [Route("Logout")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();

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

        // Notify RazorPages app to clear session (logout synchronization)
        try
        {
            var adminAppUrl = "https://localhost:7142/Auth/ClearSession";
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5); // Short timeout

            // Try to send request and wait briefly, but don't block on failure
            var sendTask = httpClient.GetAsync(adminAppUrl);
            var delayTask = Task.Delay(100); // Small delay to allow request to start
            await Task.WhenAny(sendTask, delayTask); // Wait for either to complete

            if (sendTask.IsCompleted)
            {
                if (sendTask.IsFaulted || sendTask.IsCanceled)
                {
                    Console.WriteLine(
                        $"[Logout Sync] Failed to notify admin app: {sendTask.Exception?.GetBaseException()?.Message}"
                    );
                }
                else
                {
                    Console.WriteLine(
                        "[Logout Sync] Successfully notified RazorPages app to clear session"
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
                            $"[Logout Sync] Failed to notify admin app: {task.Exception?.GetBaseException()?.Message}"
                        );
                    }
                    else
                    {
                        Console.WriteLine(
                            "[Logout Sync] Successfully notified RazorPages app to clear session"
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

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Endpoint to clear session (for logout synchronization from RazorPages app)
    /// </summary>
    [Route("ClearSession")]
    [HttpGet]
    public IActionResult ClearSession()
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

        Console.WriteLine("[ClearSession] Session cleared in MVC app");

        // Return success response
        return new JsonResult(new { success = true, message = "Session cleared" });
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
