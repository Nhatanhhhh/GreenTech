using DAL.DTOs.Category;

namespace BLL.Service.Interface
{
    public interface ICategoryService
    {
        // Basic CRUD operations
        Task<CategoryDTO> GetByIdAsync(int id);
        Task<IEnumerable<CategoryDTO>> GetAllAsync();
        Task<IEnumerable<CategoryDTO>> GetWithQueryAsync(CategoryQueryParams queryParams);
        Task<CategoryDTO> CreateAsync(CreateCategoryDTO createDto);
        Task<CategoryDTO> UpdateAsync(int id, UpdateCategoryDTO updateDto);
        Task<bool> DeleteAsync(int id);

        // Category-specific operations
        Task<IEnumerable<CategoryDTO>> GetRootCategoriesAsync();
        Task<IEnumerable<CategoryDTO>> GetSubCategoriesAsync(int parentId);
        Task<IEnumerable<CategoryDTO>> GetActiveCategoriesAsync();
        Task<IEnumerable<CategoryTreeDTO>> GetCategoryTreeAsync();

        // Validation operations
        Task<bool> CanDeleteAsync(int id);
    }
}
