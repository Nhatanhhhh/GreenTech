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
    public class CreateModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        private readonly IHubContext<CategoryHub> _categoryHubContext;

        public CreateModel(ICategoryService categoryService, IHubContext<CategoryHub> categoryHubContext)
        {
            _categoryService = categoryService;
            _categoryHubContext = categoryHubContext;
        }

        [BindProperty]
        public CreateCategoryDTO Category { get; set; } = new CreateCategoryDTO();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public SelectList ParentCategories { get; set; } =
            new SelectList(new List<SelectListItem>());

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
                // Handle local image upload if provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "categories");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    // Save relative path for serving statically
                    Category.Image = $"/uploads/categories/{uniqueFileName}";
                }
                else if (string.IsNullOrWhiteSpace(Category.Image))
                {
                    ModelState.AddModelError("ImageFile", "Vui lòng chọn hình ảnh cho danh mục.");
                    await LoadParentCategories();
                    return Page();
                }

                var created = await _categoryService.CreateAsync(Category);

                await _categoryHubContext.Clients.All.SendAsync("CategoryChanged", new
                {
                    action = "created",
                    categoryId = created.Id,
                    name = created.Name,
                    slug = created.Slug,
                    image = created.Image,
                    parentCategoryName = (string?)null,
                    productsCount = 0,
                    sortOrder = created.SortOrder,
                    isActive = created.IsActive
                });
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
