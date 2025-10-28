using BLL.Service.Interface;
using DAL.DTOs.Category;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GreenTech.Pages.Categories
{
    [AdminAuthorize]
    public class CreateModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public CreateModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public CreateCategoryDTO Category { get; set; } = new CreateCategoryDTO();

        public SelectList ParentCategories { get; set; } = new SelectList(new List<SelectListItem>());

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadParentCategories();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadParentCategories();
                return Page();
            }

            try
            {
                await _categoryService.CreateAsync(Category);
                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadParentCategories();
                return Page();
            }
        }

        private async Task LoadParentCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            ParentCategories = new SelectList(categories, "Id", "Name");
        }
    }
}

