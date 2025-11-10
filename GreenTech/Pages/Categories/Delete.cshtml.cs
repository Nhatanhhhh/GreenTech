using BLL.Service.Category.Interface;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using GreenTech.Hubs;

namespace GreenTech.Pages.Categories
{
    [AdminAuthorize]
    public class DeleteModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        private readonly IHubContext<CategoryHub> _categoryHubContext;

        public DeleteModel(ICategoryService categoryService, IHubContext<CategoryHub> categoryHubContext)
        {
            _categoryService = categoryService;
            _categoryHubContext = categoryHubContext;
        }

        public DAL.DTOs.Category.CategoryDTO? Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Category = await _categoryService.GetByIdAsync(Id);
            if (Category == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await _categoryService.DeleteAsync(Id);
                await _categoryHubContext.Clients.All.SendAsync("CategoryChanged", new
                {
                    action = "deleted",
                    categoryId = Id
                });
                return RedirectToPage("./Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                Category = await _categoryService.GetByIdAsync(Id);
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Lỗi khi xóa danh mục.");
                Category = await _categoryService.GetByIdAsync(Id);
                return Page();
            }
        }
    }
}
