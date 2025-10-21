using DAL.Context;
using DAL.DTOs.Category;
using DAL.Models;
using DAL.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Category> GetByIdAsync(int id, bool includeRelations = false)
        {
            var query = _context.Categories.AsQueryable();

            if (includeRelations)
            {
                query = query
                    .Include(c => c.ParentCategory)
                    .Include(c => c.SubCategories)
                    .Include(c => c.Products);
            }

            return await query.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetWithQueryAsync(CategoryQueryParams queryParams)
        {
            var query = _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.Products)
                .AsQueryable();

            // Filter by search keyword
            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                var searchLower = queryParams.Search.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchLower) ||
                    c.Description.ToLower().Contains(searchLower));
            }

            // Filter by active status
            if (queryParams.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == queryParams.IsActive.Value);
            }

            // Filter by parent category
            if (queryParams.ParentId.HasValue)
            {
                query = query.Where(c => c.ParentId == queryParams.ParentId.Value);
            }

            // Filter categories with/without parent
            if (queryParams.HasParent.HasValue)
            {
                if (queryParams.HasParent.Value)
                {
                    query = query.Where(c => c.ParentId != null);
                }
                else
                {
                    query = query.Where(c => c.ParentId == null);
                }
            }

            // Sorting
            query = queryParams.SortBy?.ToLower() switch
            {
                "name" => queryParams.SortDescending
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name),
                "sortorder" => queryParams.SortDescending
                    ? query.OrderByDescending(c => c.SortOrder)
                    : query.OrderBy(c => c.SortOrder),
                "createdat" => queryParams.SortDescending
                    ? query.OrderByDescending(c => c.CreatedAt)
                    : query.OrderBy(c => c.CreatedAt),
                _ => query.OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
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

        public async Task<Category> CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            var existingCategory = await _context.Categories.FindAsync(category.Id);
            if (existingCategory == null) return null;

            existingCategory.Name = category.Name;
            existingCategory.Slug = category.Slug;
            existingCategory.ParentId = category.ParentId;
            existingCategory.Image = category.Image;
            existingCategory.Description = category.Description;
            existingCategory.IsActive = category.IsActive;
            existingCategory.SortOrder = category.SortOrder;
            existingCategory.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return existingCategory;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            var hasSubCategories = await HasSubCategoriesAsync(id);
            var hasProducts = await HasProductsAsync(id);

            if (hasSubCategories || hasProducts)
            {
                // Soft delete - just deactivate
                category.IsActive = false;
                category.UpdatedAt = DateTime.Now;
            }
            else
            {
                // Hard delete - remove from database
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
        {
            var query = _context.Categories.Where(c => c.Slug == slug);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentId == null && c.IsActive)
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentId)
        {
            return await _context.Categories
                .Where(c => c.ParentId == parentId && c.IsActive)
                .Include(c => c.Products)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> HasSubCategoriesAsync(int categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.ParentId == categoryId);
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
        }

        public async Task<int> GetProductCountAsync(int categoryId)
        {
            return await _context.Products.CountAsync(p => p.CategoryId == categoryId);
        }

        public async Task<IEnumerable<Category>> GetCategoryTreeAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentId == null)
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.SubCategories)
                .Include(c => c.Products)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> IsCircularReferenceAsync(int categoryId, int? newParentId)
        {
            if (!newParentId.HasValue) return false;
            if (categoryId == newParentId.Value) return true;

            var currentParentId = newParentId;
            var maxDepth = 10; // Prevent infinite loop
            var depth = 0;

            while (currentParentId.HasValue && depth < maxDepth)
            {
                if (currentParentId.Value == categoryId)
                    return true;

                var parent = await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == currentParentId.Value);

                currentParentId = parent?.ParentId;
                depth++;
            }

            return false;
        }
    }
}
