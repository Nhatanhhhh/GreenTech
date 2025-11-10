using BLL.Service.User.Interface;
using DAL.DTOs.User;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;



namespace GreenTech.Pages.Profile
{
    [AdminAuthorize]
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;

        public IndexModel(IUserService userService)
        {
            _userService = userService;
        }

        public UserResponseDTO Profile { get; set; }
        public UpdateProfileDTO UpdateProfileModel { get; set; }
        public ChangePasswordDTO ChangePasswordModel { get; set; }
        public bool IsAdmin { get; set; }

        public async Task OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue || userId.Value == 0)
            {
                RedirectToPage("/Auth/Login");
                return;
            }

            // Check if user is Admin
            var userRoles = HttpContext.Session.GetString("UserRoles");
            IsAdmin = !string.IsNullOrEmpty(userRoles) && userRoles.Contains("ROLE_ADMIN");

            Profile = await _userService.GetProfileAsync(userId.Value);
            UpdateProfileModel = new UpdateProfileDTO
            {
                FullName = Profile.FullName,
                Email = Profile.Email,
                Phone = Profile.Phone,
                Province = Profile.Province,
                District = Profile.District,
                Ward = Profile.Ward,
                SpecificAddress = Profile.SpecificAddress,
            };
            ChangePasswordModel = new ChangePasswordDTO();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync(UpdateProfileDTO model)
        {
            // Check if user is Admin
            var userRoles = HttpContext.Session.GetString("UserRoles");
            var isAdmin = !string.IsNullOrEmpty(userRoles) && userRoles.Contains("ROLE_ADMIN");

            if (!isAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện thao tác này.";
                return RedirectToPage();
            }

            if (!ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue)
                {
                    Profile = await _userService.GetProfileAsync(userId.Value);
                    IsAdmin = isAdmin;
                }
                UpdateProfileModel = model;
                ChangePasswordModel = new ChangePasswordDTO();
                return Page();
            }

            var userIdValue = HttpContext.Session.GetInt32("UserId");
            if (!userIdValue.HasValue || userIdValue.Value == 0)
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                Profile = await _userService.UpdateProfileAsync(userIdValue.Value, model);
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";

                // Update session
                HttpContext.Session.SetString("UserName", Profile.FullName);
                HttpContext.Session.SetString("UserEmail", Profile.Email);

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                Profile = await _userService.GetProfileAsync(userIdValue.Value);
                UpdateProfileModel = model;
                ChangePasswordModel = new ChangePasswordDTO();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostChangePasswordAsync(ChangePasswordDTO model)
        {
            // Check if user is Admin
            var userRoles = HttpContext.Session.GetString("UserRoles");
            var isAdmin = !string.IsNullOrEmpty(userRoles) && userRoles.Contains("ROLE_ADMIN");

            if (!isAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện thao tác này.";
                return RedirectToPage();
            }

            if (!ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue)
                {
                    Profile = await _userService.GetProfileAsync(userId.Value);
                    IsAdmin = isAdmin;
                }
                UpdateProfileModel = new UpdateProfileDTO();
                ChangePasswordModel = model;
                return Page();
            }

            var userIdValue = HttpContext.Session.GetInt32("UserId");
            if (!userIdValue.HasValue || userIdValue.Value == 0)
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                await _userService.ChangePasswordAsync(userIdValue.Value, model);
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                Profile = await _userService.GetProfileAsync(userIdValue.Value);
                UpdateProfileModel = new UpdateProfileDTO
                {
                    FullName = Profile.FullName,
                    Email = Profile.Email,
                    Phone = Profile.Phone,
                    Province = Profile.Province,
                    District = Profile.District,
                    Ward = Profile.Ward,
                    SpecificAddress = Profile.SpecificAddress,
                };
                ChangePasswordModel = model;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUploadAvatarAsync(IFormFile avatarFile)
        {
            // Staff and Admin can both upload avatar

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue || userId.Value == 0)
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                return RedirectToPage("/Auth/Login");
            }

            if (avatarFile == null || avatarFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file ảnh.";
                return RedirectToPage();
            }

            try
            {
                var avatarUrl = await _userService.UploadAvatarAsync(userId.Value, avatarFile);
                TempData["SuccessMessage"] = "Cập nhật ảnh đại diện thành công!";

                // Update session
                HttpContext.Session.SetString("UserAvatar", avatarUrl);

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToPage();
            }
        }
    }
}
