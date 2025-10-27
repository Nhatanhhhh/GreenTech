using BLL.Service.Interface;
using DAL.DTOs.User;
using DAL.Models.Enum;
using Microsoft.AspNetCore.Mvc;

[Route("Auth")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
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
                    var redirectUrl = $"{adminAppUrl}?userId={userId}&email={Uri.EscapeDataString(userEmail)}&userName={Uri.EscapeDataString(userName)}&roles={Uri.EscapeDataString(userRoles)}";
                    
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
        return RedirectToAction("Index", "Home");
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
