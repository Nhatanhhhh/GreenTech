using DAL.DTOs.User;

namespace BLL.Service.Auth.Interface
{
    public interface IAuthService
    {
        Task<UserResponseDTO> RegisterAsync(RegisterDTO registerDTO);
        Task<UserResponseDTO> LoginAsync(LoginDTO loginDTO);
        Task LogoutAsync();
    }
}
