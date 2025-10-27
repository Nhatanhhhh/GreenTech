using BLL.Service.Interface;
using DAL.DTOs.Blog;
using GreenTech.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GreenTech.Pages.Blogs
{
    [AdminAuthorize]
    public class CreateModel : PageModel
    {
        private readonly IBlogService _blogService;
        private readonly ICategoryService _categoryService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateModel(IBlogService blogService, ICategoryService categoryService, IHttpContextAccessor httpContextAccessor)
        {
            _blogService = blogService;
            _categoryService = categoryService;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty]
        public CreateBlogDTO Blog { get; set; } = new CreateBlogDTO();

        public SelectList Categories { get; set; } = new SelectList(new List<SelectListItem>());

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCategories();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadCategories();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId") ?? 0;
                if (userId == 0)
                {
                    ModelState.AddModelError("", "Không tìm thấy thông tin người dùng.");
                    return Page();
                }

                await _blogService.CreateBlogAsync(Blog, userId);
                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi tạo bài viết.");
                return Page();
            }
        }

        private async Task LoadCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            Categories = new SelectList(categories, "Id", "Name");
        }
    }
}

