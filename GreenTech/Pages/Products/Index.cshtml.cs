using BLL.Service.Category.Interface;
using BLL.Service.Product.Interface;
using DAL.DTOs.Category;
using DAL.DTOs.Product;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Products
{
    [AdminAuthorize]
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public IndexModel(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public IEnumerable<ProductResponseDTO> Products { get; set; } =
            new List<ProductResponseDTO>();
        public IEnumerable<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        public async Task OnGetAsync()
        {
            // Get categories for filter dropdown
            Categories = await _categoryService.GetAllAsync();

            // Build query params
            var queryParams = new ProductQueryParamsDTO
            {
                PageNumber = 1,
                PageSize = 50,
                SearchTerm = SearchTerm,
                CategoryId = CategoryId,
            };

            Products = await _productService.GetProductsAsync(queryParams);
        }
    }
}
