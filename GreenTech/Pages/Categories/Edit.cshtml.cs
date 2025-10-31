using BLL.Service.Category.Interface;
using DAL.DTOs.Category;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GreenTech.Pages.Categories
{
    [AdminAuthorize]
    public class EditModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public EditModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public UpdateCategoryDTO Category { get; set; } = new UpdateCategoryDTO();

        public CategoryDTO? CategoryDetails { get; set; }
        public SelectList ParentCategories { get; set; } =
            new SelectList(new List<SelectListItem>());

        public async Task<IActionResult> OnGetAsync()
        {
            CategoryDetails = await _categoryService.GetByIdAsync(Id);
            if (CategoryDetails == null)
            {
                return NotFound();
            }

            Category = new UpdateCategoryDTO
            {
                Name = CategoryDetails.Name,
                Slug = CategoryDetails.Slug,
                ParentId = CategoryDetails.ParentId,
                Image = CategoryDetails.Image,
                Description = CategoryDetails.Description,
                IsActive = CategoryDetails.IsActive,
                SortOrder = CategoryDetails.SortOrder,
            };

            await LoadParentCategories();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadParentCategories();
                CategoryDetails = await _categoryService.GetByIdAsync(Id);
                return Page();
            }

            try
            {
                await _categoryService.UpdateAsync(Id, Category);
                return RedirectToPage("./Index");
            }
            catch (KeyNotFoundException)
            {
                ModelState.AddModelError("", "Danh mục không tồn tại.");
                await LoadParentCategories();
                CategoryDetails = await _categoryService.GetByIdAsync(Id);
                return Page();
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadParentCategories();
                CategoryDetails = await _categoryService.GetByIdAsync(Id);
                return Page();
            }
        }

        private async Task LoadParentCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            ParentCategories = new SelectList(categories.Where(c => c.Id != Id), "Id", "Name");
        }
    }
}
