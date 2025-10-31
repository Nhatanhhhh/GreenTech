using DAL.DTOs.Banner;
using BannerModel = DAL.Models.Banner;

namespace DAL.Repositories.Banner.Interface
{
    public interface IBannerRepository
    {
        // Basic CRUD operations
        Task<BannerModel> GetByIdAsync(int id, bool includeRelations = false);
        Task<IEnumerable<BannerModel>> GetAllAsync();
        Task<IEnumerable<BannerModel>> GetWithQueryAsync(BannerQueryParams queryParams);
        Task<BannerModel> CreateAsync(BannerModel banner);
        Task<BannerModel> UpdateAsync(BannerModel banner);
        Task<bool> DeleteAsync(int id);

        // Validation methods
        Task<bool> ExistsAsync(int id);

        // Banner-specific methods
        Task<IEnumerable<BannerModel>> GetActiveBannersAsync();
        Task<IEnumerable<BannerModel>> GetBannersByPositionAsync(string position);
        Task<IEnumerable<BannerModel>> GetActiveBannersByPositionAsync(string position);
        Task<int> GetMaxSortOrderAsync(string position);
        Task<bool> IncrementClickCountAsync(int bannerId);

        // Sort methods
        Task<bool> UpdateSortOrdersAsync(Dictionary<int, int> bannerSortOrders);
    }
}
