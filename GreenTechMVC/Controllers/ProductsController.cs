using BLL.Service.Category.Interface;
using BLL.Service.Product.Interface;
using DAL.DTOs.Category;
using DAL.DTOs.Product;
using Microsoft.AspNetCore.Mvc;

namespace GreenTechMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            ILogger<ProductsController> logger
        )
        {
            _productService = productService;
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(
            string search,
            int? categoryId,
            int? minPrice,
            int? maxPrice,
            string sortBy = "name",
            string sortOrder = "asc",
            int page = 1,
            int pageSize = 12
        )
        {
            var queryParams = new ProductQueryParamsDTO
            {
                SearchTerm = search,
                CategoryId = categoryId,
                IsActive = true,
                SortBy = sortBy,
                SortOrder = sortOrder,
                PageNumber = page,
                PageSize = pageSize,
            };

            var products = await _productService.GetProductsAsync(queryParams);
            var totalCount = await _productService.GetProductsCountAsync(queryParams);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            ViewBag.Products = products;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.SearchTerm = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Get related products
            var queryParams = new ProductQueryParamsDTO
            {
                CategoryId = product.CategoryId,
                IsActive = true,
                PageNumber = 1,
                PageSize = 4,
            };
            var relatedProducts = await _productService.GetProductsAsync(queryParams);

            ViewBag.Product = product;
            ViewBag.RelatedProducts = relatedProducts.Where(p => p.Id != id).Take(3).ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index", new { search });
        }
    }
}
