using DAL.DTOs.User;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface IAuthRepository
    {
        Task<User> RegisterAsync(RegisterDTO registerDTO);
        Task<User?> LoginAsync(LoginDTO loginDTO);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> PhoneExistsAsync(string phone);
    }
}
