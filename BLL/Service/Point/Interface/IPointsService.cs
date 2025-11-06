namespace BLL.Service.Point.Interface
{
    public interface IPointsService
    {
        Task<int> GetPointsBalanceAsync(int userId);
        Task<int> CalculatePointsForOrderAsync(decimal orderTotal);
        Task AwardPointsForOrderAsync(int userId, int orderId, decimal orderTotal);
    }
}
