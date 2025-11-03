using System.ComponentModel.DataAnnotations;
using BLL.Service.Order.Interface;
using BLL.Service.User.Interface;
using DAL.DTOs.User;
using GreenTechMVC.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GreenTechMVC.Controllers
{
    [Route("Profile")]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IUserService userService,
            IOrderService orderService,
            IHubContext<NotificationHub> hubContext,
            ILogger<ProfileController> logger
        )
        {
            _userService = userService;
            _orderService = orderService;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Lấy UserId từ session
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdFromSession = HttpContext.Session.GetInt32("UserId");
            if (userIdFromSession.HasValue)
            {
                return userIdFromSession.Value;
            }
            return 0;
        }

        /// <summary>
        /// Hiển thị trang profile của user
        /// </summary>
        [Route("Index")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Check if user has customer role
            var userRoles = HttpContext.Session.GetString("UserRoles");
            if (string.IsNullOrEmpty(userRoles) || !userRoles.Contains("ROLE_CUSTOMER"))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var profile = await _userService.GetProfileAsync(userId);

                // Get recent orders (last 5 orders) to display in profile
                var recentOrders = await _orderService.GetMyOrdersAsync(userId);
                ViewBag.RecentOrders = recentOrders.Take(5).ToList();

                return View(profile);
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"[Error] GetProfile - User not found: {ex.Message}");
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] GetProfile failed: {ex.Message}");
                return RedirectToAction("Login", "Auth");
            }
        }

        /// <summary>
        /// Hiển thị form cập nhật profile
        /// </summary>
        [Route("Update")]
        [HttpGet]
        public async Task<IActionResult> Update()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Check if user has customer role
            var userRoles = HttpContext.Session.GetString("UserRoles");
            if (string.IsNullOrEmpty(userRoles) || !userRoles.Contains("ROLE_CUSTOMER"))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var profile = await _userService.GetProfileAsync(userId);
                var updateDTO = new UpdateProfileDTO
                {
                    FullName = profile.FullName,
                    Email = profile.Email,
                    Phone = profile.Phone,
                    Province = profile.Province,
                    District = profile.District,
                    Ward = profile.Ward,
                    SpecificAddress = profile.SpecificAddress,
                };
                return View(updateDTO);
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"[Error] GetProfile for Update - User not found: {ex.Message}");
                return RedirectToAction("Login", "Auth");
            }
        }

        /// <summary>
        /// Xử lý cập nhật profile
        /// </summary>
        [Route("Update")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateProfileDTO model)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Check if user has customer role
            var userRoles = HttpContext.Session.GetString("UserRoles");
            if (string.IsNullOrEmpty(userRoles) || !userRoles.Contains("ROLE_CUSTOMER"))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get current profile to preserve Email and Phone (these fields cannot be updated)
            var currentProfile = await _userService.GetProfileAsync(userId);
            if (currentProfile == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy thông tin người dùng.");
                return View(model);
            }

            // Preserve Email and Phone from database (not from form)
            model.Email = currentProfile.Email;
            model.Phone = currentProfile.Phone;

            // Remove Email and Phone validation errors since we're using database values
            ModelState.Remove("Email");
            ModelState.Remove("Phone");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var updatedProfile = await _userService.UpdateProfileAsync(userId, model);

                // Update session with new name if changed
                if (updatedProfile.FullName != HttpContext.Session.GetString("UserName"))
                {
                    HttpContext.Session.SetString("UserName", updatedProfile.FullName);
                }

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Index");
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã có lỗi xảy ra khi cập nhật thông tin.");
                Console.WriteLine($"[Error] UpdateProfile failed: {ex.Message}");
            }

            return View(model);
        }

        /// <summary>
        /// Hiển thị form đổi mật khẩu
        /// </summary>
        [Route("ChangePassword")]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Check if user has customer role
            var userRoles = HttpContext.Session.GetString("UserRoles");
            if (string.IsNullOrEmpty(userRoles) || !userRoles.Contains("ROLE_CUSTOMER"))
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(new ChangePasswordDTO());
        }

        /// <summary>
        /// Xử lý đổi mật khẩu
        /// </summary>
        [Route("ChangePassword")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Check if user has customer role
            var userRoles = HttpContext.Session.GetString("UserRoles");
            if (string.IsNullOrEmpty(userRoles) || !userRoles.Contains("ROLE_CUSTOMER"))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _userService.ChangePasswordAsync(userId, model);
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                // Parse validation errors from ValidationException
                // Format: "Error1; Error2; ..."
                // Errors from VerifyCurrentPasswordAttribute will be: "Mật khẩu hiện tại không chính xác"
                var errorMessages = ex.Message.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var errorMessage in errorMessages)
                {
                    var trimmedError = errorMessage.Trim();

                    // Check if error is for CurrentPassword field (from VerifyCurrentPasswordAttribute)
                    // The error message is "Mật khẩu hiện tại không chính xác"
                    if (trimmedError.Contains("Mật khẩu hiện tại"))
                    {
                        // Add to CurrentPassword field for inline display
                        ModelState.AddModelError(nameof(model.CurrentPassword), trimmedError);
                    }
                    else if (!string.IsNullOrWhiteSpace(trimmedError))
                    {
                        // Add other validation errors to general model errors
                        ModelState.AddModelError(string.Empty, trimmedError);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã có lỗi xảy ra khi đổi mật khẩu.");
                Console.WriteLine($"[Error] ChangePassword failed: {ex.Message}");
            }

            return View(model);
        }

        /// <summary>
        /// Upload avatar
        /// </summary>
        [Route("UploadAvatar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            if (avatarFile == null || avatarFile.Length == 0)
            {
                return Json(new { success = false, message = "File ảnh không được để trống." });
            }

            try
            {
                var avatarUrl = await _userService.UploadAvatarAsync(userId, avatarFile);

                // Send realtime notification via SignalR to update avatar immediately
                var groupName = $"user-{userId}";
                _logger.LogInformation(
                    "Sending SignalR notification to group '{GroupName}' for avatar update. UserId: {UserId}, AvatarUrl: {AvatarUrl}",
                    groupName,
                    userId,
                    avatarUrl
                );

                try
                {
                    await _hubContext
                        .Clients.Group(groupName)
                        .SendAsync(
                            "AvatarUpdated",
                            new
                            {
                                userId = userId,
                                avatarUrl = avatarUrl,
                                timestamp = DateTime.UtcNow,
                            }
                        );
                    _logger.LogInformation(
                        "SignalR notification sent successfully to group '{GroupName}'",
                        groupName
                    );
                }
                catch (Exception signalrEx)
                {
                    // Log but don't fail the upload
                    _logger.LogWarning(
                        signalrEx,
                        "Failed to send SignalR notification to group '{GroupName}'. Avatar upload still succeeded.",
                        groupName
                    );
                }

                // Session đã được update trong UserService
                return Json(
                    new
                    {
                        success = true,
                        message = "Upload avatar thành công!",
                        avatarUrl = avatarUrl,
                    }
                );
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] UploadAvatar failed: {ex.Message}");
                return Json(
                    new { success = false, message = "Đã có lỗi xảy ra khi upload avatar." }
                );
            }
        }

        /// <summary>
        /// Hiển thị form quên mật khẩu
        /// </summary>
        [Route("ForgotPassword")]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordDTO());
        }

        /// <summary>
        /// Xử lý quên mật khẩu
        /// </summary>
        [Route("ForgotPassword")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _userService.ForgotPasswordAsync(model);

                TempData["SuccessMessage"] =
                    "Mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư và nhập mã OTP để tiếp tục.";
                TempData["Email"] = model.Email;

                return RedirectToAction("VerifyOTP", new { email = model.Email });
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Đã có lỗi xảy ra khi xử lý yêu cầu quên mật khẩu."
                );
                Console.WriteLine($"[Error] ForgotPassword failed: {ex.Message}");
            }

            return View(model);
        }

        /// <summary>
        /// Hiển thị form xác thực OTP
        /// </summary>
        [Route("VerifyOTP")]
        [HttpGet]
        public IActionResult VerifyOTP(string email = null)
        {
            var model = new VerifyOTPDTO { Email = email ?? TempData["Email"]?.ToString() ?? "" };

            if (string.IsNullOrEmpty(model.Email))
            {
                TempData["ErrorMessage"] = "Email không hợp lệ. Vui lòng thử lại.";
                return RedirectToAction("ForgotPassword");
            }

            return View(model);
        }

        /// <summary>
        /// Xử lý xác thực OTP
        /// </summary>
        [Route("VerifyOTP")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOTP(VerifyOTPDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _userService.VerifyOTPAsync(model);

                // Store email in session for ResetPassword
                HttpContext.Session.SetString("OTPVerifiedEmail", model.Email);
                TempData["SuccessMessage"] =
                    "Xác thực OTP thành công! Vui lòng đặt lại mật khẩu mới.";

                return RedirectToAction("ResetPassword", new { email = model.Email });
            }
            catch (UnauthorizedAccessException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã có lỗi xảy ra khi xác thực OTP.");
                Console.WriteLine($"[Error] VerifyOTP failed: {ex.Message}");
            }

            return View(model);
        }

        /// <summary>
        /// Hiển thị form đặt lại mật khẩu
        /// </summary>
        [Route("ResetPassword")]
        [HttpGet]
        public IActionResult ResetPassword(string email = null)
        {
            // Check if OTP has been verified
            var verifiedEmail = HttpContext.Session.GetString("OTPVerifiedEmail");
            var emailToUse = email ?? verifiedEmail ?? "";

            if (string.IsNullOrEmpty(emailToUse))
            {
                TempData["ErrorMessage"] = "Vui lòng xác thực OTP trước khi đặt lại mật khẩu.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordDTO { Email = emailToUse };

            return View(model);
        }

        /// <summary>
        /// Xử lý đặt lại mật khẩu
        /// </summary>
        [Route("ResetPassword")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO model)
        {
            // Check if OTP has been verified
            var verifiedEmail = HttpContext.Session.GetString("OTPVerifiedEmail");
            if (string.IsNullOrEmpty(verifiedEmail) || verifiedEmail != model.Email)
            {
                TempData["ErrorMessage"] = "Vui lòng xác thực OTP trước khi đặt lại mật khẩu.";
                return RedirectToAction("VerifyOTP", new { email = model.Email });
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _userService.ResetPasswordAsync(model);

                // Clear verified email from session
                HttpContext.Session.Remove("OTPVerifiedEmail");

                TempData["SuccessMessage"] =
                    "Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";
                return RedirectToAction("Login", "Auth/Login");
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã có lỗi xảy ra khi đặt lại mật khẩu.");
                Console.WriteLine($"[Error] ResetPassword failed: {ex.Message}");
            }

            return View(model);
        }
    }
}
