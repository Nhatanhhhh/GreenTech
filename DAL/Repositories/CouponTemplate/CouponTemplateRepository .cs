using DAL.Context;
using DAL.DTOs.CouponTemplate;
using DAL.Repositories.CouponTemplate.Interface;
using Microsoft.EntityFrameworkCore;
using CouponTemplateModel = DAL.Models.CouponTemplate;

namespace DAL.Repositories.CouponTemplate
{
    public class CouponTemplateRepository : ICouponTemplateRepository
    {
        private readonly AppDbContext _context;

        public CouponTemplateRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CouponTemplateModel> GetByIdAsync(int id)
        {
            return await _context.CouponTemplates.FirstOrDefaultAsync(ct => ct.Id == id);
        }

        public async Task<IEnumerable<CouponTemplateModel>> GetAllAsync()
        {
            return await _context
                .CouponTemplates.Where(ct => ct.IsActive)
                .OrderBy(ct => ct.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<CouponTemplateModel>> GetWithQueryAsync(
            CouponTemplateQueryParams queryParams
        )
        {
            var query = _context.CouponTemplates.AsQueryable();

            // Filter by search
            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                query = query.Where(ct =>
                    ct.Name.Contains(queryParams.Search)
                    || ct.Description.Contains(queryParams.Search)
                );
            }

            // Filter by active status
            if (queryParams.IsActive.HasValue)
            {
                query = query.Where(ct => ct.IsActive == queryParams.IsActive.Value);
            }

            // Sorting
            query = queryParams.SortBy?.ToLower() switch
            {
                "name" => queryParams.SortDescending
                    ? query.OrderByDescending(ct => ct.Name)
                    : query.OrderBy(ct => ct.Name),
                "pointscost" => queryParams.SortDescending
                    ? query.OrderByDescending(ct => ct.PointsCost)
                    : query.OrderBy(ct => ct.PointsCost),
                "createdat" => queryParams.SortDescending
                    ? query.OrderByDescending(ct => ct.CreatedAt)
                    : query.OrderBy(ct => ct.CreatedAt),
                _ => queryParams.SortDescending
                    ? query.OrderByDescending(ct => ct.Name)
                    : query.OrderBy(ct => ct.Name),
            };

            // Pagination
            if (queryParams.Page > 0 && queryParams.PageSize > 0)
            {
                query = query
                    .Skip((queryParams.Page - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize);
            }

            return await query.ToListAsync();
        }

        public async Task<CouponTemplateModel> CreateAsync(CouponTemplateModel couponTemplate)
        {
            _context.CouponTemplates.Add(couponTemplate);
            await _context.SaveChangesAsync();
            return couponTemplate;
        }

        public async Task<CouponTemplateModel> UpdateAsync(CouponTemplateModel couponTemplate)
        {
            _context.CouponTemplates.Update(couponTemplate);
            await _context.SaveChangesAsync();
            return couponTemplate;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var template = await _context.CouponTemplates.FindAsync(id);
            if (template == null)
                return false;

            var hasCoupons = await _context.Coupons.AnyAsync(c => c.TemplateId == id);
            if (hasCoupons)
            {
                // Soft delete
                template.IsActive = false;
            }
            else
            {
                _context.CouponTemplates.Remove(template);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.CouponTemplates.AnyAsync(ct => ct.Id == id);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.CouponTemplates.Where(ct => ct.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(ct => ct.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> GetTotalUsageCountAsync(int templateId)
        {
            return await _context.Coupons.Where(c => c.TemplateId == templateId).CountAsync();
        }
    }
}
