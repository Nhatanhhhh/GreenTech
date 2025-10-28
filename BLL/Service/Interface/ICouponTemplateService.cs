using DAL.DTOs.CouponTemplate;

namespace BLL.Service.Interface
{
    public interface ICouponTemplateService
    {
        Task<CouponTemplateDTO> GetByIdAsync(int id);
        Task<IEnumerable<CouponTemplateDTO>> GetAllAsync();
        Task<IEnumerable<CouponTemplateDTO>> GetWithQueryAsync(CouponTemplateQueryParams queryParams);
        Task<CouponTemplateDTO> CreateAsync(CreateCouponTemplateDTO createDto);
        Task<CouponTemplateDTO> UpdateAsync(int id, CouponTemplateDTO updateDto);
        Task<bool> DeleteAsync(int id);
    }
}
