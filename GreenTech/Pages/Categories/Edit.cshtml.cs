using BLL.Service.Category.Interface;
using DAL.DTOs.Category;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.SignalR;
using GreenTech.Hubs;

namespace GreenTech.Pages.Categories
{
    [AdminAuthorize]
    public class EditModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        private readonly IHubContext<CategoryHub> _categoryHubContext;

        public EditModel(ICategoryService categoryService, IHubContext<CategoryHub> categoryHubContext)
        {
            _categoryService = categoryService;
            _categoryHubContext = categoryHubContext;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public UpdateCategoryDTO Category { get; set; } = new UpdateCategoryDTO();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

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
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var current = await _categoryService.GetByIdAsync(Id);
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "categories");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    if (!string.IsNullOrWhiteSpace(current?.Image) && current.Image.StartsWith("/uploads/categories/", StringComparison.OrdinalIgnoreCase))
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", current.Image.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(oldPath))
                        {
                            try { System.IO.File.Delete(oldPath); } catch { }
                        }
                    }
                    Category.Image = $"/uploads/categories/{uniqueFileName}";
                }
                var updated = await _categoryService.UpdateAsync(Id, Category);

                await _categoryHubContext.Clients.All.SendAsync("CategoryChanged", new
                {
                    action = "updated",
                    categoryId = updated.Id,
                    name = updated.Name,
                    slug = updated.Slug,
                    image = updated.Image,
                    parentCategoryName = updated.ParentCategoryName,
                    productsCount = updated.ProductsCount,
                    sortOrder = updated.SortOrder,
                    isActive = updated.IsActive
                });
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
