using BLL.Service.Point.Interface;
using DAL.Repositories.Point.Interface;

namespace BLL.Service.Point
{
    public class PointsService : IPointsService
    {
        private readonly IPointsRepository _pointsRepository;

        public PointsService(IPointsRepository pointsRepository)
        {
            _pointsRepository = pointsRepository;
        }

        public Task<int> GetPointsBalanceAsync(int userId)
        {
            return _pointsRepository.GetPointsBalanceAsync(userId);
        }
    }
}
