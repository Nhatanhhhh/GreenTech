using BLL.Service.Blog.Interface;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Blogs
{
    [AdminAuthorize]
    public class DeleteModel : PageModel
    {
        private readonly IBlogService _blogService;

        public DeleteModel(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public DAL.DTOs.Blog.BlogResponseDTO? Blog { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Blog = await _blogService.GetBlogByIdAsync(Id);
            if (Blog == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await _blogService.DeleteBlogAsync(Id);
                return RedirectToPage("./Index");
            }
            catch (KeyNotFoundException)
            {
                ModelState.AddModelError("", "Không tìm thấy bài viết để xóa.");
                Blog = await _blogService.GetBlogByIdAsync(Id);
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi xóa bài viết: {ex.Message}");
                Blog = await _blogService.GetBlogByIdAsync(Id);
                return Page();
            }
        }
    }
}
