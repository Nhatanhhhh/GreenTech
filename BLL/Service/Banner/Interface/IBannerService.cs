using DAL.DTOs.Banner;
using DAL.Models;

namespace BLL.Service.Banner.Interface
{
    public interface IBannerService
    {
        // Basic CRUD operations
        Task<BannerDTO> GetByIdAsync(int id);
        Task<IEnumerable<BannerDTO>> GetAllAsync();
        Task<IEnumerable<BannerDTO>> GetWithQueryAsync(BannerQueryParams queryParams);
        Task<BannerDTO> CreateAsync(CreateBannerDTO createDto, int createdBy);
        Task<BannerDTO> UpdateAsync(int id, UpdateBannerDTO updateDto);
        Task<bool> DeleteAsync(int id);

        // Banner-specific operations
        Task<IEnumerable<BannerDTO>> GetActiveBannersAsync();
        Task<IEnumerable<BannerDTO>> GetBannersByPositionAsync(string position);
        Task<IEnumerable<BannerDTO>> GetActiveBannersByPositionAsync(string position);
        Task<bool> IncrementClickCountAsync(int bannerId);

        // Sort operations
        Task<bool> UpdateSortOrdersAsync(Dictionary<int, int> bannerSortOrders);
    }
}
