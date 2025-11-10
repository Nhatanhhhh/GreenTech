using DAL.DTOs.User;
using Microsoft.AspNetCore.Http;

namespace BLL.Service.User.Interface
{
    public interface IUserService
    {
        Task<UserResponseDTO> GetProfileAsync(int userId);
        Task<UserResponseDTO> UpdateProfileAsync(int userId, UpdateProfileDTO updateProfileDTO);
        Task ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDTO);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDTO);
        Task<bool> VerifyOTPAsync(VerifyOTPDTO verifyOTPDTO);
        Task ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO);
        Task<string> UploadAvatarAsync(int userId, IFormFile avatarFile);
        Task<(IEnumerable<UserResponseDTO> Users, int TotalCount)> GetAllUsersAsync(
            UserQueryParamsDTO queryParams
        );
        Task<UserResponseDTO> GetUserDetailsAsync(int userId);
        Task<UserResponseDTO> BlockUserAsync(int userId);
        Task<UserResponseDTO> UnblockUserAsync(int userId);
        Task<UserResponseDTO> ChangeUserRoleAsync(int userId, string roleName);
    }
}
