using BLL.Service.Blog.Interface;
using DAL.DTOs.Blog;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Blogs
{
    [AdminAuthorize]
    public class IndexModel : PageModel
    {
        private readonly IBlogService _blogService;

        public IndexModel(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public IEnumerable<BlogResponseDTO> Blogs { get; set; } = new List<BlogResponseDTO>();
        public BlogQueryParamsDTO QueryParams { get; set; } = new BlogQueryParamsDTO();
        public int TotalBlogs { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalBlogs / QueryParams.PageSize);
        public bool HasPreviousPage => QueryParams.PageNumber > 1;
        public bool HasNextPage => QueryParams.PageNumber < TotalPages;

        public async Task OnGetAsync(int pageNumber = 1)
        {
            QueryParams.PageNumber = pageNumber;
            QueryParams.PageSize = 10;

            Blogs = await _blogService.GetBlogsAsync(QueryParams);
            TotalBlogs = await _blogService.GetBlogsCountAsync(QueryParams);
        }
    }
}
