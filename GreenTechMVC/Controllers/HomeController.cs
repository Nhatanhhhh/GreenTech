using BLL.Service.Banner.Interface;
using BLL.Service.Blog.Interface;
using BLL.Service.Category.Interface;
using BLL.Service.Product.Interface;
using DAL.DTOs.Banner;
using DAL.DTOs.Blog;
using DAL.DTOs.Category;
using DAL.DTOs.Product;
using Microsoft.AspNetCore.Mvc;

namespace GreenTechMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBannerService _bannerService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBlogService _blogService;

        public HomeController(
            ILogger<HomeController> logger,
            IBannerService bannerService,
            IProductService productService,
            ICategoryService categoryService,
            IBlogService blogService
        )
        {
            _logger = logger;
            _bannerService = bannerService;
            _productService = productService;
            _categoryService = categoryService;
            _blogService = blogService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Banners = await _bannerService.GetActiveBannersByPositionAsync("HOME_SLIDER");
            ViewBag.FeaturedProducts = await GetFeaturedProducts();
            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Blogs = await GetFeaturedBlogs();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        private async Task<List<ProductResponseDTO>> GetFeaturedProducts()
        {
            var queryParams = new ProductQueryParamsDTO
            {
                IsActive = true,
                IsFeatured = true,
                PageNumber = 1,
                PageSize = 8,
            };
            var products = await _productService.GetProductsAsync(queryParams);
            return products.ToList();
        }

        private async Task<List<BlogResponseDTO>> GetFeaturedBlogs()
        {
            var queryParams = new BlogQueryParamsDTO
            {
                IsPublished = true,
                IsFeatured = true,
                PageNumber = 1,
                PageSize = 3,
            };
            var blogs = await _blogService.GetBlogsAsync(queryParams);
            return blogs.ToList();
        }
    }
}
