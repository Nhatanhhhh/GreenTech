namespace DAL.Repositories.Point.Interface
{
    public interface IPointsRepository
    {
        Task<int> GetPointsBalanceAsync(int userId);
    }
}
