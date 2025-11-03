using DAL.DTOs.User;
using DAL.Models;
using UserModel = DAL.Models.User;

namespace DAL.Repositories.User.Interface
{
    public interface IUserRepository
    {
        Task<UserModel?> GetByIdAsync(int userId);
        Task<UserModel?> GetByIdAsync(int userId, bool includeBlocked);
        Task<UserModel?> GetByEmailAsync(string email);
        Task<UserModel> UpdateAsync(UserModel user);
        Task<IEnumerable<UserModel>> GetAllAsync(UserQueryParamsDTO queryParams);
        Task<int> CountAsync(UserQueryParamsDTO queryParams);
        Task<UserModel> BlockUserAsync(int userId);
        Task<UserModel> UnblockUserAsync(int userId);
        Task<UserModel> ChangeUserRoleAsync(int userId, string roleName);
    }
}
