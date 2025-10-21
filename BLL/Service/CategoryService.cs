using BLL.Service.Interface;
using DAL.DTOs.Category;
using DAL.Repositories.Interface;
using DAL.Utils.AutoMapper;
using DAL.Utils.ValidationHelper;

namespace BLL.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<CategoryDTO> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id, includeRelations: true);
            if (category == null) return null;

            return AutoMapper.ToCategoryDTO(category);
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return AutoMapper.ToCategoryDTOs(categories);
        }

        public async Task<IEnumerable<CategoryDTO>> GetWithQueryAsync(CategoryQueryParams queryParams)
        {
            var categories = await _categoryRepository.GetWithQueryAsync(queryParams);
            return AutoMapper.ToCategoryDTOs(categories);
        }

        public async Task<CategoryDTO> CreateAsync(CreateCategoryDTO createDto)
        {
            // Validate input
            ValidationHelper.ValidateModel(createDto);

            // Check if slug already exists
            if (await _categoryRepository.SlugExistsAsync(createDto.Slug))
                throw new ArgumentException("Slug đã tồn tại trong hệ thống");

            // Validate parent category if provided
            if (createDto.ParentId.HasValue)
            {
                if (!await _categoryRepository.ExistsAsync(createDto.ParentId.Value))
                    throw new ArgumentException("Danh mục cha không tồn tại");
            }

            // Map DTO to Entity
            var category = AutoMapper.ToCategory(createDto);

            // Create category
            var createdCategory = await _categoryRepository.CreateAsync(category);

            // Return DTO
            return AutoMapper.ToCategoryDTO(createdCategory);
        }

        public async Task<CategoryDTO> UpdateAsync(int id, UpdateCategoryDTO updateDto)
        {
            // Validate input
            ValidationHelper.ValidateModel(updateDto);

            // Check if category exists
            var existingCategory = await _categoryRepository.GetByIdAsync(id)
                ?? throw new ArgumentException("Danh mục không tồn tại");

            // Check if slug is unique (excluding current category)
            if (await _categoryRepository.SlugExistsAsync(updateDto.Slug, id))
                throw new ArgumentException("Slug đã tồn tại trong hệ thống");

            // Validate parent category
            if (updateDto.ParentId.HasValue)
            {
                // Check if parent category exists
                if (!await _categoryRepository.ExistsAsync(updateDto.ParentId.Value))
                    throw new ArgumentException("Danh mục cha không tồn tại");

                // Check for circular reference
                if (await _categoryRepository.IsCircularReferenceAsync(id, updateDto.ParentId.Value))
                    throw new ArgumentException("Không thể đặt danh mục con làm danh mục cha (tham chiếu vòng)");
            }

            // Map DTO to Entity
            var categoryToUpdate = AutoMapper.ToCategory(updateDto);
            categoryToUpdate.Id = id;
            categoryToUpdate.CreatedAt = existingCategory.CreatedAt;

            // Update category
            var updatedCategory = await _categoryRepository.UpdateAsync(categoryToUpdate);

            // Return DTO
            return AutoMapper.ToCategoryDTO(updatedCategory);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Check if category exists
            if (!await _categoryRepository.ExistsAsync(id))
                throw new ArgumentException("Danh mục không tồn tại");

            // Check if can delete
            var canDelete = await CanDeleteAsync(id);
            if (!canDelete)
            {
                throw new InvalidOperationException(
                    "Không thể xóa danh mục này vì đang có danh mục con hoặc sản phẩm. " +
                    "Danh mục sẽ được vô hiệu hóa thay vì xóa hoàn toàn.");
            }

            return await _categoryRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<CategoryDTO>> GetRootCategoriesAsync()
        {
            var categories = await _categoryRepository.GetRootCategoriesAsync();
            return AutoMapper.ToCategoryDTOs(categories);
        }

        public async Task<IEnumerable<CategoryDTO>> GetSubCategoriesAsync(int parentId)
        {
            // Check if parent category exists
            if (!await _categoryRepository.ExistsAsync(parentId))
                throw new ArgumentException("Danh mục cha không tồn tại");

            var categories = await _categoryRepository.GetSubCategoriesAsync(parentId);
            return AutoMapper.ToCategoryDTOs(categories);
        }

        public async Task<IEnumerable<CategoryDTO>> GetActiveCategoriesAsync()
        {
            var categories = await _categoryRepository.GetActiveCategoriesAsync();
            return AutoMapper.ToCategoryDTOs(categories);
        }

        public async Task<IEnumerable<CategoryTreeDTO>> GetCategoryTreeAsync()
        {
            var categories = await _categoryRepository.GetCategoryTreeAsync();
            return AutoMapper.ToCategoryTreeDTOs(categories);
        }

        public async Task<bool> CanDeleteAsync(int id)
        {
            var hasSubCategories = await _categoryRepository.HasSubCategoriesAsync(id);
            var hasProducts = await _categoryRepository.HasProductsAsync(id);

            return !hasSubCategories && !hasProducts;
        }
    }
}
