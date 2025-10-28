using BLL.Service.Interface;
using DAL.DTOs.CouponTemplate;
using DAL.Models.Enum;
using DAL.Repositories.Interface;
using DAL.Utils.AutoMapper;
using DAL.Utils.ValidationHelper;

namespace BLL.Service
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
            if (template == null) return null;

            return AutoMapper.ToCouponTemplateDTO(template);
        }

        public async Task<IEnumerable<CouponTemplateDTO>> GetAllAsync()
        {
            var templates = await _couponTemplateRepository.GetAllAsync();
            return templates.Select(t => AutoMapper.ToCouponTemplateDTO(t));
        }

        public async Task<IEnumerable<CouponTemplateDTO>> GetWithQueryAsync(CouponTemplateQueryParams queryParams)
        {
            var templates = await _couponTemplateRepository.GetWithQueryAsync(queryParams);
            return templates.Select(t => AutoMapper.ToCouponTemplateDTO(t));
        }

        public async Task<CouponTemplateDTO> CreateAsync(CreateCouponTemplateDTO createDto)
        {
            ValidationHelper.ValidateModel(createDto);

            if (await _couponTemplateRepository.NameExistsAsync(createDto.Name))
                throw new ArgumentException("Tên template đã tồn tại");

            if (createDto.DiscountType == DiscountType.PERCENT && createDto.DiscountValue > 100)
                throw new ArgumentException("Giảm giá theo phần trăm không được vượt quá 100%");

            var template = AutoMapper.ToCouponTemplate(createDto);
            var createdTemplate = await _couponTemplateRepository.CreateAsync(template);

            return AutoMapper.ToCouponTemplateDTO(createdTemplate);
        }

        public async Task<CouponTemplateDTO> UpdateAsync(int id, CouponTemplateDTO updateDto)
        {
            ValidationHelper.ValidateModel(updateDto);

            var existingTemplate = await _couponTemplateRepository.GetByIdAsync(id)
                ?? throw new ArgumentException("Template không tồn tại");

            if (await _couponTemplateRepository.NameExistsAsync(updateDto.Name, id))
                throw new ArgumentException("Tên template đã tồn tại");

            if (updateDto.DiscountType == DiscountType.PERCENT && updateDto.DiscountValue > 100)
                throw new ArgumentException("Giảm giá theo phần trăm không được vượt quá 100%");

            var templateToUpdate = AutoMapper.ToCouponTemplate(updateDto);
            templateToUpdate.Id = id;

            var updatedTemplate = await _couponTemplateRepository.UpdateAsync(templateToUpdate);
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
