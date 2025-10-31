using DAL.DTOs.CouponTemplate;
using CouponTemplateModel = DAL.Models.CouponTemplate;

namespace DAL.Repositories.CouponTemplate.Interface
{
    public interface ICouponTemplateRepository
    {
        Task<CouponTemplateModel> GetByIdAsync(int id);
        Task<IEnumerable<CouponTemplateModel>> GetAllAsync();
        Task<IEnumerable<CouponTemplateModel>> GetWithQueryAsync(
            CouponTemplateQueryParams queryParams
        );
        Task<CouponTemplateModel> CreateAsync(CouponTemplateModel couponTemplate);
        Task<CouponTemplateModel> UpdateAsync(CouponTemplateModel couponTemplate);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<int> GetTotalUsageCountAsync(int templateId);
    }
}
