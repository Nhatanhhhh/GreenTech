using BLL.Service.User.Interface;
using DAL.DTOs.User;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Users
{
    [AdminAuthorize]
    public class DetailsModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(
            IUserService userService,
            IHttpClientFactory httpClientFactory,
            ILogger<DetailsModel> logger
        )
        {
            _userService = userService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public new UserResponseDTO User { get; set; }
        public string Message { get; set; }
        public string MessageType { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                User = await _userService.GetUserDetailsAsync(id);
                if (User == null)
                {
                    TempData["Error"] = "Người dùng không tồn tại";
                    return RedirectToPage("./Index");
                }

                // Kiểm tra nếu user là Admin, không cho phép xem
                if (User.Roles.Contains("ROLE_ADMIN"))
                {
                    TempData["Error"] = "Không được phép xem thông tin Admin";
                    return RedirectToPage("./Index");
                }
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Người dùng không tồn tại";
                return RedirectToPage("./Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostBlockAsync(int id)
        {
            try
            {
                var userBefore = await _userService.GetUserDetailsAsync(id);
                if (userBefore == null)
                {
                    TempData["Error"] = "Người dùng không tồn tại";
                    return RedirectToPage("./Index");
                }

                // Kiểm tra nếu user là Admin
                if (userBefore.Roles.Contains("ROLE_ADMIN"))
                {
                    TempData["Error"] = "Không được phép chặn Admin";
                    return RedirectToPage("./Index");
                }

                User = await _userService.BlockUserAsync(id);
                TempData["Success"] = $"Đã chặn người dùng {User.FullName}";

                // Gửi SignalR notification đến MVC để logout user ngay lập tức
                await SendUserBanNotificationAsync(id, isBanned: true, User.FullName);
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Người dùng không tồn tại";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnPostUnblockAsync(int id)
        {
            try
            {
                var userBefore = await _userService.GetUserDetailsAsync(id);
                if (userBefore == null)
                {
                    TempData["Error"] = "Người dùng không tồn tại";
                    return RedirectToPage("./Index");
                }

                User = await _userService.UnblockUserAsync(id);
                TempData["Success"] = $"Đã bỏ chặn người dùng {User.FullName}";

                // Gửi SignalR notification đến MVC để thông báo user đã được unban
                await SendUserBanNotificationAsync(id, isBanned: false, User.FullName);
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Người dùng không tồn tại";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnPostChangeRoleAsync(int id, string roleName)
        {
            try
            {
                var userBefore = await _userService.GetUserDetailsAsync(id);
                if (userBefore == null)
                {
                    TempData["Error"] = "Người dùng không tồn tại";
                    return RedirectToPage("./Index");
                }

                // Kiểm tra nếu user là Admin
                if (userBefore.Roles.Contains("ROLE_ADMIN"))
                {
                    TempData["Error"] = "Không được phép thay đổi role của Admin";
                    return RedirectToPage("./Index");
                }

                // Không cho phép đổi thành Admin
                if (roleName == "ROLE_ADMIN")
                {
                    TempData["Error"] = "Không được phép thay đổi role thành ADMIN";
                    return RedirectToPage("./Details", new { id });
                }

                User = await _userService.ChangeUserRoleAsync(id, roleName);
                TempData["Success"] =
                    $"Đã thay đổi role của {User.FullName} thành {roleName.Replace("ROLE_", "")}";
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Người dùng không tồn tại";
                return RedirectToPage("./Index");
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage("./Details", new { id });
        }

        private async Task SendUserBanNotificationAsync(int userId, bool isBanned, string fullName)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var mvcBaseUrl = "http://localhost:5045"; // MVC base URL

                var notification = new
                {
                    userId = userId,
                    isBanned = isBanned,
                    fullName = fullName,
                    message = isBanned
                        ? "Tài khoản của bạn đã bị chặn bởi quản trị viên."
                        : "Tài khoản của bạn đã được bỏ chặn.",
                };

                var response = await httpClient.PostAsJsonAsync(
                    $"{mvcBaseUrl}/api/UserNotification/user-banned",
                    notification
                );

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Successfully sent user ban notification for UserId={UserId}, IsBanned={IsBanned}",
                        userId,
                        isBanned
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to send user ban notification. StatusCode={StatusCode}",
                        response.StatusCode
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending user ban notification");
            }
        }
    }
}
