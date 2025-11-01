using DAL.DTOs.User;
using UserModel = DAL.Models.User;

namespace DAL.Repositories.Auth.Interface
{
    public interface IAuthRepository
    {
        Task<UserModel> RegisterAsync(RegisterDTO registerDTO);
        Task<UserModel?> LoginAsync(LoginDTO loginDTO);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> PhoneExistsAsync(string phone);
    }
}
