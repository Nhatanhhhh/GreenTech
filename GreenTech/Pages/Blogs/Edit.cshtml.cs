using BLL.Service.Interface;
using DAL.DTOs.Blog;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GreenTech.Pages.Blogs
{
    [AdminAuthorize]
    public class EditModel : PageModel
    {
        private readonly IBlogService _blogService;
        private readonly ICategoryService _categoryService;

        public EditModel(IBlogService blogService, ICategoryService categoryService)
        {
            _blogService = blogService;
            _categoryService = categoryService;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public UpdateBlogDTO Blog { get; set; } = new UpdateBlogDTO();

        public BlogResponseDTO? BlogDetails { get; set; }
        public SelectList Categories { get; set; } = new SelectList(new List<SelectListItem>());

        public async Task<IActionResult> OnGetAsync()
        {
            BlogDetails = await _blogService.GetBlogByIdAsync(Id);
            if (BlogDetails == null)
            {
                return NotFound();
            }

            Blog = new UpdateBlogDTO
            {
                Title = BlogDetails.Title,
                Excerpt = BlogDetails.Excerpt,
                Content = BlogDetails.Content,
                CategoryId = BlogDetails.CategoryId,
                Tags = BlogDetails.Tags,
                IsFeatured = BlogDetails.IsFeatured,
                IsPublished = BlogDetails.IsPublished,
                SeoTitle = "",
                SeoDescription = ""
            };

            await LoadCategories();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                BlogDetails = await _blogService.GetBlogByIdAsync(Id);
                return Page();
            }

            try
            {
                await _blogService.UpdateBlogAsync(Id, Blog);
                return RedirectToPage("./Index");
            }
            catch (KeyNotFoundException)
            {
                ModelState.AddModelError("", "Bài viết không tồn tại.");
                await LoadCategories();
                BlogDetails = await _blogService.GetBlogByIdAsync(Id);
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

