using DAL.Models;
using UserModel = DAL.Models.User;

namespace DAL.Repositories.Point.Interface
{
    public interface IPointsRepository
    {
        Task<int> GetPointsBalanceAsync(int userId);
        Task<PointEarningRule?> GetActivePointEarningRuleAsync(decimal orderAmount);
        Task<PointTransaction> CreatePointTransactionAsync(PointTransaction pointTransaction);
        Task<UserModel?> GetUserByIdAsync(int userId);
        Task UpdateUserPointsAsync(int userId, int points);
    }
}
