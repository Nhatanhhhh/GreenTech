using DAL.Models;
using UserModel = DAL.Models.User;

namespace DAL.Repositories.User.Interface
{
    public interface IUserRepository
    {
        Task<UserModel?> GetByIdAsync(int userId);
        Task<UserModel?> GetByEmailAsync(string email);
        Task<UserModel> UpdateAsync(UserModel user);
    }
}
