using BLL.Service.CouponTemplate.Interface;
using DAL.DTOs.CouponTemplate;
using DAL.Models.Enum;
using DAL.Repositories.CouponTemplate.Interface;
using DAL.Utils.AutoMapper;
using DAL.Utils.ValidationHelper;

namespace BLL.Service.CouponTemplate
{
    public class CouponTemplateService : ICouponTemplateService
    {
        private readonly ICouponTemplateRepository _couponTemplateRepository;

        public CouponTemplateService(ICouponTemplateRepository couponTemplateRepository)
        {
            _couponTemplateRepository = couponTemplateRepository;
        }

        public async Task<CouponTemplateDTO> GetByIdAsync(int id)
        {
            var template = await _couponTemplateRepository.GetByIdAsync(id);
            if (template == null)
                return null;

            return AutoMapper.ToCouponTemplateDTO(template);
        }

        public async Task<IEnumerable<CouponTemplateDTO>> GetAllAsync()
        {
            var templates = await _couponTemplateRepository.GetAllAsync();
            return templates.Select(t => AutoMapper.ToCouponTemplateDTO(t));
        }

        public async Task<IEnumerable<CouponTemplateDTO>> GetWithQueryAsync(
            CouponTemplateQueryParams queryParams
        )
        {
            var templates = await _couponTemplateRepository.GetWithQueryAsync(queryParams);
            return templates.Select(t => AutoMapper.ToCouponTemplateDTO(t));
        }

        public async Task<CouponTemplateDTO> CreateAsync(CreateCouponTemplateDTO createDto)
        {
            // ValidateModel sẽ check tất cả validation attributes từ DTO (bao gồm [DiscountValue], [Required], etc.)
            ValidationHelper.ValidateModel(createDto);

            if (await _couponTemplateRepository.NameExistsAsync(createDto.Name))
                throw new ArgumentException("Tên template đã tồn tại");

            // Không cần validate lại DiscountValue vì đã có [DiscountValue] attribute trong DTO

            var template = AutoMapper.ToCouponTemplate(createDto);
            var createdTemplate = await _couponTemplateRepository.CreateAsync(template);

            return AutoMapper.ToCouponTemplateDTO(createdTemplate);
        }

        public async Task<CouponTemplateDTO> UpdateAsync(int id, CouponTemplateDTO updateDto)
        {
            // ValidateModel sẽ check tất cả validation attributes từ DTO (bao gồm [DiscountValue], [Required], etc.)
            ValidationHelper.ValidateModel(updateDto);

            var existingTemplate =
                await _couponTemplateRepository.GetByIdAsync(id)
                ?? throw new ArgumentException("Template không tồn tại");

            if (await _couponTemplateRepository.NameExistsAsync(updateDto.Name, id))
                throw new ArgumentException("Tên template đã tồn tại");

            // Không cần validate lại DiscountValue vì đã có [DiscountValue] attribute trong DTO

            // Update existing entity properties instead of creating new one
            // This preserves CreatedAt and avoids EF tracking issues
            existingTemplate.Name = updateDto.Name;
            existingTemplate.Description = updateDto.Description;
            existingTemplate.DiscountType = updateDto.DiscountType;
            existingTemplate.DiscountValue = updateDto.DiscountValue;
            existingTemplate.MinOrderAmount = updateDto.MinOrderAmount;
            existingTemplate.PointsCost = updateDto.PointsCost;
            existingTemplate.UsageLimitPerUser = updateDto.UsageLimitPerUser;
            existingTemplate.TotalUsageLimit = updateDto.TotalUsageLimit;
            existingTemplate.IsActive = updateDto.IsActive;
            existingTemplate.ValidDays = updateDto.ValidDays;
            // CreatedAt is preserved automatically

            var updatedTemplate = await _couponTemplateRepository.UpdateAsync(existingTemplate);
            return AutoMapper.ToCouponTemplateDTO(updatedTemplate);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _couponTemplateRepository.ExistsAsync(id))
                throw new ArgumentException("Template không tồn tại");

            return await _couponTemplateRepository.DeleteAsync(id);
        }
    }
}
