using DAL.Context;
using DAL.Models;
using DAL.Repositories.Point.Interface;
using Microsoft.EntityFrameworkCore;
using UserModel = DAL.Models.User;

namespace DAL.Repositories.Point
{
    public class PointsRepository : IPointsRepository
    {
        private readonly AppDbContext _dbContext;

        public PointsRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> GetPointsBalanceAsync(int userId)
        {
            var user = await _dbContext
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");
            return user.Points;
        }

        public async Task<PointEarningRule?> GetActivePointEarningRuleAsync(decimal orderAmount)
        {
            var now = DateTime.Now;
            return await _dbContext
                .PointEarningRules.Where(rule =>
                    rule.IsActive
                    && rule.ValidFrom <= now
                    && (rule.ValidUntil == null || rule.ValidUntil >= now)
                    && rule.MinOrderAmount <= orderAmount
                )
                .OrderByDescending(rule => rule.PointsPerAmount)
                .ThenByDescending(rule => rule.MinOrderAmount)
                .FirstOrDefaultAsync();
        }

        public async Task<PointTransaction> CreatePointTransactionAsync(
            PointTransaction pointTransaction
        )
        {
            _dbContext.PointTransactions.Add(pointTransaction);
            await _dbContext.SaveChangesAsync();
            return pointTransaction;
        }

        public async Task<UserModel?> GetUserByIdAsync(int userId)
        {
            return await _dbContext.Users.FindAsync(userId);
        }

        public async Task UpdateUserPointsAsync(int userId, int points)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.Points = points;
                user.UpdatedAt = DateTime.Now;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
