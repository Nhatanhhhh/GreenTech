using System.ComponentModel.DataAnnotations;
using BLL.Service.User.Interface;
using DAL.DTOs.User;
using Microsoft.AspNetCore.Mvc;

namespace GreenTechMVC.Controllers
{
    [Route("Profile")]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
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
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem thông tin cá nhân.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var profile = await _userService.GetProfileAsync(userId);
                return View(profile);
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã có lỗi xảy ra khi tải thông tin profile.";
                Console.WriteLine($"[Error] GetProfile failed: {ex.Message}");
                return RedirectToAction("Index", "Home");
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
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để cập nhật thông tin.";
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
                TempData["ErrorMessage"] = ex.Message;
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
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để cập nhật thông tin.";
                return RedirectToAction("Login", "Auth");
            }

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
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để đổi mật khẩu.";
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
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để đổi mật khẩu.";
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
                return RedirectToAction("Login", "Auth");
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
