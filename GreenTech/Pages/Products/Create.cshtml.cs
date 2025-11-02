using BLL.Service.Category.Interface;
using BLL.Service.Product.Interface;
using BLL.Service.Supplier.Interface;
using DAL.DTOs.Category;
using DAL.DTOs.Product;
using DAL.DTOs.Supplier;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GreenTech.Pages.Products
{
    [AdminAuthorize]
    public class CreateModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;

        public CreateModel(
            IProductService productService,
            ICategoryService categoryService,
            ISupplierService supplierService
        )
        {
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
        }

        [BindProperty]
        public CreateProductDTO Product { get; set; } = new CreateProductDTO();

        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> Suppliers { get; set; } = new();

        public async Task OnGetAsync()
        {
            var categories = await _categoryService.GetAllAsync();
            Categories = categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            var suppliers = await _supplierService.GetSuppliersAsync(
                new DAL.DTOs.Supplier.SupplierQueryParamsDTO { PageNumber = 1, PageSize = 100 }
            );
            Suppliers = suppliers
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            // Ensure session is available before checking
            if (!HttpContext.Session.IsAvailable)
            {
                await HttpContext.Session.LoadAsync();
            }

            // Check authentication before processing
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            var userRoles = HttpContext.Session.GetString("UserRoles");

            // If session is expired, reload the page with error instead of redirecting to login
            if (string.IsNullOrEmpty(isAuthenticated) || isAuthenticated != "true")
            {
                await LoadSelectLists();
                ModelState.AddModelError(
                    string.Empty,
                    "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
                );
                return Page();
            }

            if (string.IsNullOrEmpty(userRoles) || !userRoles.Contains("ROLE_ADMIN"))
            {
                await LoadSelectLists();
                ModelState.AddModelError(
                    string.Empty,
                    "Bạn không có quyền thực hiện thao tác này."
                );
                return Page();
            }

            // Ensure Description and Dimensions are not null
            if (Product != null)
            {
                Product.Description = Product.Description ?? string.Empty;
                Product.Dimensions = Product.Dimensions ?? string.Empty;
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return Page();
            }

            try
            {
                await _productService.CreateProductAsync(Product);
                TempData["SuccessMessage"] = "Tạo sản phẩm thành công!";
                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadSelectLists();
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi tạo sản phẩm.");
                await LoadSelectLists();
                return Page();
            }
        }

        private async Task LoadSelectLists()
        {
            var categories = await _categoryService.GetAllAsync();
            Categories = categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();

            var suppliers = await _supplierService.GetSuppliersAsync(
                new DAL.DTOs.Supplier.SupplierQueryParamsDTO { PageNumber = 1, PageSize = 100 }
            );
            Suppliers = suppliers
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToList();
        }
    }
}
