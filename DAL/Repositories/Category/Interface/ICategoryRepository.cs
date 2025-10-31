using System.Threading.Tasks;
using DAL.DTOs.Category;
using CategoryModel = DAL.Models.Category;

namespace DAL.Repositories.Category.Interface
{
    public interface ICategoryRepository
    {
        // Basic CRUD operations
        Task<CategoryModel> GetByIdAsync(int id, bool includeRelations = false);
        Task<IEnumerable<CategoryModel>> GetAllAsync();
        Task<IEnumerable<CategoryModel>> GetWithQueryAsync(CategoryQueryParams queryParams);
        Task<CategoryModel> CreateAsync(CategoryModel category);
        Task<CategoryModel> UpdateAsync(CategoryModel category);
        Task<bool> DeleteAsync(int id);

        // Validation methods
        Task<bool> ExistsAsync(int id);
        Task<bool> SlugExistsAsync(string slug, int? excludeId = null);

        // Category-specific methods
        Task<IEnumerable<CategoryModel>> GetRootCategoriesAsync();
        Task<IEnumerable<CategoryModel>> GetSubCategoriesAsync(int parentId);
        Task<IEnumerable<CategoryModel>> GetActiveCategoriesAsync();
        Task<bool> HasSubCategoriesAsync(int categoryId);
        Task<bool> HasProductsAsync(int categoryId);
        Task<int> GetProductCountAsync(int categoryId);

        // Tree structure methods
        Task<IEnumerable<CategoryModel>> GetCategoryTreeAsync();
        Task<bool> IsCircularReferenceAsync(int categoryId, int? newParentId);
    }
}
