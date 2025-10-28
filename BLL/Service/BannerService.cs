using BLL.Service.Interface;
using DAL.DTOs.Banner;
using DAL.Repositories.Interface;
using DAL.Utils.AutoMapper;

namespace BLL.Service
{
    public class BannerService : IBannerService
    {
        private readonly IBannerRepository _bannerRepository;

        public BannerService(IBannerRepository bannerRepository)
        {
            _bannerRepository = bannerRepository;
        }

        public async Task<BannerDTO> GetByIdAsync(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id, includeRelations: true);
            if (banner == null)
            {
                return null;
            }
            return AutoMapper.ToBannerDTO(banner);
        }

        public async Task<IEnumerable<BannerDTO>> GetAllAsync()
        {
            var banners = await _bannerRepository.GetAllAsync();
            return AutoMapper.ToBannerDTOs(banners);
        }

        public async Task<IEnumerable<BannerDTO>> GetWithQueryAsync(BannerQueryParams queryParams)
        {
            var banners = await _bannerRepository.GetWithQueryAsync(queryParams);
            return AutoMapper.ToBannerDTOs(banners);
        }

        public async Task<BannerDTO> CreateAsync(CreateBannerDTO createDto, int createdBy)
        {
            var banner = AutoMapper.ToBanner(createDto);

            banner.CreatedBy = createdBy;
            banner.CreatedAt = DateTime.Now;
            banner.UpdatedAt = DateTime.Now;
            banner.ClickCount = 0;

            if (banner.SortOrder <= 0)
            {
                var maxSortOrder = await _bannerRepository.GetMaxSortOrderAsync(banner.Position.ToString());
                banner.SortOrder = maxSortOrder + 1;
            }

            var createdBanner = await _bannerRepository.CreateAsync(banner);

            var resultBanner = await _bannerRepository.GetByIdAsync(createdBanner.Id, includeRelations: true);

            return AutoMapper.ToBannerDTO(resultBanner);
        }

        public async Task<BannerDTO> UpdateAsync(int id, UpdateBannerDTO updateDto)
        {
            var existingBanner = await _bannerRepository.GetByIdAsync(id, includeRelations: true); // Lấy cả Creator
            if (existingBanner == null)
            {
                return null;
            }

            AutoMapper.ApplyUpdatesToBanner(updateDto, existingBanner);

            var updatedBanner = await _bannerRepository.UpdateAsync(existingBanner);

            return AutoMapper.ToBannerDTO(updatedBanner);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exists = await _bannerRepository.ExistsAsync(id);
            if (!exists)
            {
                return false;
            }
            return await _bannerRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<BannerDTO>> GetActiveBannersAsync()
        {
            var banners = await _bannerRepository.GetActiveBannersAsync();
            return AutoMapper.ToBannerDTOs(banners);
        }

        public async Task<IEnumerable<BannerDTO>> GetBannersByPositionAsync(string position)
        {
            var banners = await _bannerRepository.GetBannersByPositionAsync(position);
            return AutoMapper.ToBannerDTOs(banners);
        }

        public async Task<IEnumerable<BannerDTO>> GetActiveBannersByPositionAsync(string position)
        {
            var banners = await _bannerRepository.GetActiveBannersByPositionAsync(position);
            return AutoMapper.ToBannerDTOs(banners);
        }

        public async Task<bool> IncrementClickCountAsync(int bannerId)
        {
            return await _bannerRepository.IncrementClickCountAsync(bannerId);
        }

        public async Task<bool> UpdateSortOrdersAsync(Dictionary<int, int> bannerSortOrders)
        {
            return await _bannerRepository.UpdateSortOrdersAsync(bannerSortOrders);
        }
    }
}
