using BLL.Service.Point.Interface;
using DAL.Models;
using DAL.Models.Enum;
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

        public async Task<int> CalculatePointsForOrderAsync(decimal orderTotal)
        {
            // Lấy rule tích điểm đang active
            var rule = await _pointsRepository.GetActivePointEarningRuleAsync(orderTotal);

            if (rule == null)
            {
                return 0; // Không có rule nào, không tích điểm
            }

            // Tính điểm: orderTotal / PointsPerAmount (ví dụ: 100,000đ / 1 = 100 điểm)
            var points = (int)(orderTotal / rule.PointsPerAmount);

            // Áp dụng giới hạn max points nếu có
            if (rule.MaxPointsPerOrder.HasValue && points > rule.MaxPointsPerOrder.Value)
            {
                points = rule.MaxPointsPerOrder.Value;
            }

            return points;
        }

        public async Task AwardPointsForOrderAsync(int userId, int orderId, decimal orderTotal)
        {
            // Lấy user để lấy điểm hiện tại
            var user = await _pointsRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại");
            }

            // Tính điểm sẽ được cộng
            var pointsToAward = await CalculatePointsForOrderAsync(orderTotal);

            if (pointsToAward <= 0)
            {
                return; // Không có điểm để cộng
            }

            // Lấy rule để lưu vào transaction
            var rule = await _pointsRepository.GetActivePointEarningRuleAsync(orderTotal);

            // Tính điểm trước và sau
            var pointsBefore = user.Points;
            var pointsAfter = pointsBefore + pointsToAward;

            // Tạo PointTransaction
            var transaction = new PointTransaction
            {
                UserId = userId,
                TransactionType = PointTransactionType.EARNED,
                Points = pointsToAward,
                ReferenceType = ReferenceType.ORDER,
                ReferenceId = orderId,
                PointEarningRuleId = rule?.Id,
                Description = $"Tích điểm từ đơn hàng #{orderId}",
                PointsBefore = pointsBefore,
                PointsAfter = pointsAfter,
                CreatedAt = DateTime.Now,
            };

            // Lưu transaction và cập nhật điểm user
            await _pointsRepository.CreatePointTransactionAsync(transaction);
            await _pointsRepository.UpdateUserPointsAsync(userId, pointsAfter);
        }
    }
}
