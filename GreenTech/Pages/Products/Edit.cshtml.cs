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
    public class EditModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;

        public EditModel(
            IProductService productService,
            ICategoryService categoryService,
            ISupplierService supplierService
        )
        {
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public int ProductId { get; set; }

        [BindProperty]
        public UpdateProductDTO Product { get; set; } = new UpdateProductDTO();

        public ProductResponseDTO? ProductDetails { get; set; }
        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> Suppliers { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Use includeInactive: true để có thể edit cả products đã inactive
            ProductDetails = await _productService.GetProductByIdAsync(Id, includeInactive: true);
            if (ProductDetails == null)
            {
                return NotFound();
            }

            // Initialize Product with existing data
            Product = new UpdateProductDTO
            {
                Name = ProductDetails.Name,
                Description = ProductDetails.Description ?? string.Empty,
                ShortDescription = ProductDetails.ShortDescription,
                CategoryId = ProductDetails.CategoryId,
                SupplierId = ProductDetails.SupplierId,
                CostPrice = ProductDetails.CostPrice,
                SellPrice = ProductDetails.SellPrice,
                Quantity = ProductDetails.Quantity,
                PlantSize = ProductDetails.PlantSize,
                Tags = ProductDetails.Tags,
                CareInstructions = ProductDetails.CareInstructions,
                IsFeatured = ProductDetails.IsFeatured,
                IsActive = ProductDetails.IsActive,
                PointsEarned = ProductDetails.PointsEarned,
                Dimensions = ProductDetails.Dimensions ?? string.Empty,
                Weight = ProductDetails.Weight,
            };

            // Load dropdown data
            await LoadSelectLists();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
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
                ProductDetails = await _productService.GetProductByIdAsync(
                    Id,
                    includeInactive: true
                );
                if (ProductDetails != null)
                {
                    Product.Description = ProductDetails.Description ?? string.Empty;
                    Product.Dimensions = ProductDetails.Dimensions ?? string.Empty;
                }
                ModelState.AddModelError(
                    string.Empty,
                    "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
                );
                return Page();
            }

            if (string.IsNullOrEmpty(userRoles) || !userRoles.Contains("ROLE_ADMIN"))
            {
                await LoadSelectLists();
                ProductDetails = await _productService.GetProductByIdAsync(
                    Id,
                    includeInactive: true
                );
                if (ProductDetails != null)
                {
                    Product.Description = ProductDetails.Description ?? string.Empty;
                    Product.Dimensions = ProductDetails.Dimensions ?? string.Empty;
                }
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

            // Đảm bảo logic IsActive: Update không được phép deactivate
            // Chỉ cho phép restore nếu product đang inactive
            if (ProductDetails != null && ProductDetails.IsActive && !Product.IsActive)
            {
                // Nếu product đang active, không cho phép set thành inactive qua Update
                Product.IsActive = true;
                ModelState.AddModelError(
                    "",
                    "Không thể vô hiệu hóa sản phẩm qua chức năng Cập nhật. Hãy sử dụng chức năng Xóa."
                );
            }

            if (!ModelState.IsValid)
            {
                // Log ModelState errors for debugging
                foreach (var error in ModelState)
                {
                    foreach (var errorMessage in error.Value.Errors)
                    {
                        Console.WriteLine(
                            $"Field: {error.Key}, Error: {errorMessage.ErrorMessage}"
                        );
                    }
                }

                await LoadSelectLists();
                ProductDetails = await _productService.GetProductByIdAsync(Id);

                // Reload Product data if validation fails
                if (ProductDetails != null)
                {
                    Product.Description = ProductDetails.Description ?? string.Empty;
                    Product.Dimensions = ProductDetails.Dimensions ?? string.Empty;
                    Product.Weight = ProductDetails.Weight;
                }

                return Page();
            }

            try
            {
                // Get uploaded files
                var mainImage = Request.Form.Files["MainImage"];
                var additionalImages = Request.Form.Files.GetFiles("AdditionalImages");

                IFormFile? mainImageFile = null;
                if (mainImage != null && mainImage.Length > 0)
                {
                    mainImageFile = mainImage;
                }

                List<IFormFile>? additionalImagesList = null;
                if (additionalImages != null && additionalImages.Count > 0)
                {
                    additionalImagesList = additionalImages.Where(f => f.Length > 0).ToList();
                }

                await _productService.UpdateProductAsync(
                    Id,
                    Product,
                    mainImageFile,
                    additionalImagesList
                );
                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                return RedirectToPage("./Index");
            }
            catch (KeyNotFoundException)
            {
                ModelState.AddModelError("", "Sản phẩm không tồn tại.");
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
