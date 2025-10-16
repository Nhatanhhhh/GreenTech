using DAL.DTOs.CouponTemplate;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface ICouponTemplateRepository
    {
        Task<CouponTemplate> GetByIdAsync(int id);
        Task<IEnumerable<CouponTemplate>> GetAllAsync();
        Task<IEnumerable<CouponTemplate>> GetWithQueryAsync(CouponTemplateQueryParams queryParams);
        Task<CouponTemplate> CreateAsync(CouponTemplate couponTemplate);
        Task<CouponTemplate> UpdateAsync(CouponTemplate couponTemplate);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<int> GetTotalUsageCountAsync(int templateId);
    }
}
