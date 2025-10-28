using DAL.DTOs.Category;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.Interface
{
    public interface ICategoryRepository
    {
        // Basic CRUD operations
        Task<Category> GetByIdAsync(int id, bool includeRelations = false);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<IEnumerable<Category>> GetWithQueryAsync(CategoryQueryParams queryParams);
        Task<Category> CreateAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id);

        // Validation methods
        Task<bool> ExistsAsync(int id);
        Task<bool> SlugExistsAsync(string slug, int? excludeId = null);

        // Category-specific methods
        Task<IEnumerable<Category>> GetRootCategoriesAsync();
        Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentId);
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<bool> HasSubCategoriesAsync(int categoryId);
        Task<bool> HasProductsAsync(int categoryId);
        Task<int> GetProductCountAsync(int categoryId);

        // Tree structure methods
        Task<IEnumerable<Category>> GetCategoryTreeAsync();
        Task<bool> IsCircularReferenceAsync(int categoryId, int? newParentId);
    }
}
