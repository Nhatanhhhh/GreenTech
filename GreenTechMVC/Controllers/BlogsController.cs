using BLL.Service.Blog.Interface;
using DAL.DTOs.Blog;
using Microsoft.AspNetCore.Mvc;

namespace GreenTechMVC.Controllers
{
    public class BlogsController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly ILogger<BlogsController> _logger;

        public BlogsController(IBlogService blogService, ILogger<BlogsController> logger)
        {
            _blogService = blogService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(
            string search,
            int? categoryId,
            string tag,
            string sortBy = "publishedAt",
            string sortOrder = "desc",
            int page = 1,
            int pageSize = 12
        )
        {
            var queryParams = new BlogQueryParamsDTO
            {
                SearchTerm = search,
                CategoryId = categoryId,
                Tag = tag,
                IsPublished = true, // Only show published blogs to customers
                SortBy = sortBy,
                SortOrder = sortOrder,
                PageNumber = page,
                PageSize = pageSize,
            };

            var blogs = await _blogService.GetBlogsAsync(queryParams);
            var totalBlogs = await _blogService.GetBlogsCountAsync(queryParams);
            var totalPages = (int)Math.Ceiling((double)totalBlogs / pageSize);

            ViewBag.Blogs = blogs;
            ViewBag.TotalBlogs = totalBlogs;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;
            ViewBag.SearchTerm = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.Tag = tag;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View();
        }

        public async Task<IActionResult> Details(int? id, string? slug)
        {
            if (!id.HasValue && string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            BlogResponseDTO? blog = null;

            if (id.HasValue)
            {
                blog = await _blogService.GetBlogByIdAsync(id.Value);
            }
            else if (!string.IsNullOrEmpty(slug))
            {
                blog = await _blogService.GetBlogBySlugAsync(slug);
            }

            if (blog == null || !blog.IsPublished)
            {
                return NotFound();
            }

            // Get related blogs (same category, excluding current blog)
            var relatedBlogsQuery = new BlogQueryParamsDTO
            {
                CategoryId = blog.CategoryId,
                IsPublished = true,
                PageNumber = 1,
                PageSize = 4,
                SortBy = "publishedAt",
                SortOrder = "desc",
            };

            var relatedBlogs = (await _blogService.GetBlogsAsync(relatedBlogsQuery))
                .Where(b => b.Id != blog.Id)
                .Take(3)
                .ToList();

            ViewBag.Blog = blog;
            ViewBag.RelatedBlogs = relatedBlogs;

            return View();
        }
    }
}
