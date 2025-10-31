using BLL.Service.Auth.Interface;
using DAL.DTOs.User;
using DAL.Models;
using DAL.Models.Enum;
using DAL.Repositories.Auth.Interface;
using DAL.Utils.AutoMapper;
using DAL.Utils.CryptoUtil;
using DAL.Utils.ValidationHelper;
using Microsoft.AspNetCore.Http;

namespace BLL.Service.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IAuthRepository authRepository, IHttpContextAccessor httpContextAccessor)
        {
            _authRepository = authRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserResponseDTO> RegisterAsync(RegisterDTO registerDTO)
        {
            ValidationHelper.ValidateModel(registerDTO);

            if (await _authRepository.EmailExistsAsync(registerDTO.Email))
                throw new ArgumentException("Email đã tồn tại trong hệ thống");

            if (await _authRepository.PhoneExistsAsync(registerDTO.Phone))
                throw new ArgumentException("Số điện thoại đã tồn tại trong hệ thống");

            // Hash password using HMACSHA512
            registerDTO.Password = CryptoUtil.HashPasswordHmacSHA512(registerDTO.Password);

            var user = await _authRepository.RegisterAsync(registerDTO);
            return AutoMapper.ToUserResponseDTO(user);
        }

        public async Task<UserResponseDTO> LoginAsync(LoginDTO loginDTO)
        {
            ValidationHelper.ValidateModel(loginDTO);

            var user =
                await _authRepository.LoginAsync(loginDTO)
                ?? throw new UnauthorizedAccessException("Email hoặc mật khẩu không chính xác");

            // Verify password using HMACSHA512
            if (!CryptoUtil.VerifyPasswordHmacSHA512(user.Password, loginDTO.Password))
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không chính xác");

            if (user.Status != UserStatus.ACTIVE)
                throw new UnauthorizedAccessException("Tài khoản của bạn đã bị khóa");

            SaveUserSession(user);

            return AutoMapper.ToUserResponseDTO(user);
        }

        public Task LogoutAsync()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Clear();
            return Task.CompletedTask;
        }

        private void SaveUserSession(User user)
        {
            var session =
                _httpContextAccessor.HttpContext?.Session
                ?? throw new InvalidOperationException("Session không khả dụng");

            session.SetInt32("UserId", user.Id);
            session.SetString("UserEmail", user.Email);
            session.SetString("UserName", user.FullName);
            session.SetString("IsAuthenticated", "true");

            var roles = user.UserRoles?.Select(ur => ur.Role.RoleName.ToString()).ToList();
            if (roles != null && roles.Any())
                session.SetString("UserRoles", string.Join(",", roles));
        }
    }
}
