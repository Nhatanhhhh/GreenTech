namespace BLL.Service.Point.Interface
{
    public interface IPointsService
    {
        Task<int> GetPointsBalanceAsync(int userId);
    }
}
