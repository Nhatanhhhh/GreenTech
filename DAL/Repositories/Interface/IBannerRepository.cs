using DAL.DTOs.Banner;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface IBannerRepository
    {
        // Basic CRUD operations
        Task<Banner> GetByIdAsync(int id, bool includeRelations = false);
        Task<IEnumerable<Banner>> GetAllAsync();
        Task<IEnumerable<Banner>> GetWithQueryAsync(BannerQueryParams queryParams);
        Task<Banner> CreateAsync(Banner banner);
        Task<Banner> UpdateAsync(Banner banner);
        Task<bool> DeleteAsync(int id);

        // Validation methods
        Task<bool> ExistsAsync(int id);

        // Banner-specific methods
        Task<IEnumerable<Banner>> GetActiveBannersAsync();
        Task<IEnumerable<Banner>> GetBannersByPositionAsync(string position);
        Task<IEnumerable<Banner>> GetActiveBannersByPositionAsync(string position);
        Task<int> GetMaxSortOrderAsync(string position);
        Task<bool> IncrementClickCountAsync(int bannerId);

        // Sort methods
        Task<bool> UpdateSortOrdersAsync(Dictionary<int, int> bannerSortOrders);
    }
}
