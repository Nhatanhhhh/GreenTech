using BLL.Service.Cloudinary.Interface;
using BLL.Service.Email.Interface;
using BLL.Service.OTP.Interface;
using BLL.Service.User.Interface;
using DAL.Context;
using DAL.DTOs.User;
using DAL.Repositories.User.Interface;
using DAL.Utils.AutoMapper;
using DAL.Utils.CryptoUtil;
using DAL.Utils.ValidationHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserModel = DAL.Models.User;

namespace BLL.Service.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<UserService> _logger;
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOTPService _otpService;
        private readonly IEmailService _emailService;

        public UserService(
            IUserRepository userRepository,
            IFileStorageService fileStorageService,
            ILogger<UserService> logger,
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IOTPService otpService,
            IEmailService emailService
        )
        {
            _userRepository = userRepository;
            _fileStorageService = fileStorageService;
            _logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _otpService = otpService;
            _emailService = emailService;
        }

        public async Task<UserResponseDTO> GetProfileAsync(int userId)
        {
            var user =
                await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("Người dùng không tồn tại");

            return AutoMapper.ToUserResponseDTO(user);
        }

        public async Task<UserResponseDTO> UpdateProfileAsync(
            int userId,
            UpdateProfileDTO updateProfileDTO
        )
        {
            // Set UserId for validation attributes (to exclude current user from uniqueness check)
            updateProfileDTO.UserId = userId;

            // Create service provider for validation (needed for custom attributes that access database)
            var serviceProvider = new ServiceCollection()
                .AddScoped<AppDbContext>(_ => _context)
                .BuildServiceProvider();

            ValidationHelper.ValidateModel(updateProfileDTO, serviceProvider);

            var user =
                await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("Người dùng không tồn tại");

            // Validation attributes will handle email/phone uniqueness checks

            // Update user properties
            user.FullName = updateProfileDTO.FullName;
            user.Email = updateProfileDTO.Email;
            user.Phone = updateProfileDTO.Phone;
            user.Province = updateProfileDTO.Province;
            user.District = updateProfileDTO.District;
            user.Ward = updateProfileDTO.Ward;
            user.SpecificAddress = updateProfileDTO.SpecificAddress ?? string.Empty;

            await _userRepository.UpdateAsync(user);

            return AutoMapper.ToUserResponseDTO(user);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDTO)
        {
            ValidationHelper.ValidateModel(changePasswordDTO);

            var user =
                await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("Người dùng không tồn tại");

            // Verify current password
            if (
                !CryptoUtil.VerifyPasswordHmacSHA512(
                    user.Password,
                    changePasswordDTO.CurrentPassword
                )
            )
                throw new UnauthorizedAccessException("Mật khẩu hiện tại không chính xác");

            // Hash new password
            user.Password = CryptoUtil.HashPasswordHmacSHA512(changePasswordDTO.NewPassword);
            await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDTO)
        {
            ValidationHelper.ValidateModel(forgotPasswordDTO);

            var user =
                await _userRepository.GetByEmailAsync(forgotPasswordDTO.Email)
                ?? throw new KeyNotFoundException("Email không tồn tại trong hệ thống");

            // Generate OTP (valid for 10 minutes)
            string otp = _otpService.GenerateOTP(user.Email, expiryMinutes: 10);

            // Send OTP via email
            string emailSubject = "Mã OTP đặt lại mật khẩu - GreenTech";
            string emailBody =
                $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #007934;'>Đặt lại mật khẩu</h2>
                    <p>Xin chào <strong>{user.FullName}</strong>,</p>
                    <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của mình.</p>
                    <p>Mã OTP của bạn là:</p>
                    <div style='background-color: #f0f0f0; padding: 20px; text-align: center; margin: 20px 0; border-radius: 5px;'>
                        <h1 style='color: #007934; font-size: 32px; letter-spacing: 5px; margin: 0;'>{otp}</h1>
                    </div>
                    <p>Mã OTP này có hiệu lực trong <strong>10 phút</strong>.</p>
                    <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'/>
                    <p style='color: #666; font-size: 12px;'>Email này được gửi tự động từ hệ thống GreenTech. Vui lòng không trả lời email này.</p>
                </div>
            ";

            bool emailSent = await _emailService.SendEmailAsync(
                user.Email,
                emailSubject,
                emailBody
            );

            if (!emailSent)
            {
                _logger.LogError($"Failed to send OTP email to {user.Email}");
                throw new Exception("Không thể gửi email OTP. Vui lòng thử lại sau.");
            }

            _logger.LogInformation($"OTP sent successfully to {user.Email}");
            return true;
        }

        public async Task<bool> VerifyOTPAsync(VerifyOTPDTO verifyOTPDTO)
        {
            ValidationHelper.ValidateModel(verifyOTPDTO);

            // Verify OTP
            bool isValid = _otpService.VerifyOTP(verifyOTPDTO.Email, verifyOTPDTO.OTP);

            if (!isValid)
            {
                throw new UnauthorizedAccessException("Mã OTP không hợp lệ hoặc đã hết hạn.");
            }

            // Remove OTP after successful verification
            _otpService.RemoveOTP(verifyOTPDTO.Email);

            return true;
        }

        public async Task ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO)
        {
            ValidationHelper.ValidateModel(resetPasswordDTO);

            // Get user by email
            var user =
                await _userRepository.GetByEmailAsync(resetPasswordDTO.Email)
                ?? throw new KeyNotFoundException("Email không tồn tại trong hệ thống");

            // Hash new password
            user.Password = CryptoUtil.HashPasswordHmacSHA512(resetPasswordDTO.NewPassword);
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation($"Password reset successfully for user: {user.Email}");
        }

        public async Task<string> UploadAvatarAsync(int userId, IFormFile avatarFile)
        {
            if (avatarFile == null || avatarFile.Length == 0)
                throw new ArgumentException("File ảnh không được để trống");

            // Validate file type (only images)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException(
                    "Chỉ chấp nhận file ảnh với định dạng: JPG, JPEG, PNG, GIF, WEBP"
                );

            // Validate file size (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (avatarFile.Length > maxFileSize)
                throw new ArgumentException("Kích thước file không được vượt quá 5MB");

            var user =
                await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("Người dùng không tồn tại");

            // Delete old avatar if exists
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(user.Avatar);
                }
                catch (Exception ex)
                {
                    // Log error but continue (old file might not exist or already deleted)
                    _logger.LogWarning(
                        ex,
                        "Failed to delete old avatar for user {UserId}. Avatar URL: {AvatarUrl}. Continuing with upload.",
                        userId,
                        user.Avatar
                    );
                }
            }

            // Upload new avatar using optimized image upload with Cloudinary transformations
            // - Resize to max 400x400px (maintain aspect ratio)
            // - Auto format conversion (webp when supported for better compression)
            // - Auto quality optimization
            // - Face detection gravity for better avatar cropping
            var avatarUrl = await _fileStorageService.SaveImageAsync(
                avatarFile,
                subFolder: "avatars",
                maxWidth: 400,
                maxHeight: 400,
                quality: "auto",
                format: "auto"
            );

            // Update user avatar
            user.Avatar = avatarUrl;
            await _userRepository.UpdateAsync(user);

            // Update session with new avatar URL
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.SetString("UserAvatar", avatarUrl);
            }

            return avatarUrl;
        }
    }
}
