using DAL.Context;
using DAL.Repositories.Point.Interface;
using Microsoft.EntityFrameworkCore;

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
    }
}
