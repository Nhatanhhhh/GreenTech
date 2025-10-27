using BLL.Service.Interface;
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

        public EditModel(IProductService productService, ICategoryService categoryService, ISupplierService supplierService)
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
            ProductDetails = await _productService.GetProductByIdAsync(Id);
            if (ProductDetails == null)
            {
                return NotFound();
            }

            // Initialize Product with existing data
            Product = new UpdateProductDTO
            {
                Name = ProductDetails.Name,
                Description = ProductDetails.Description,
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
                PointsEarned = ProductDetails.PointsEarned
            };

            // Load dropdown data
            await LoadSelectLists();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                ProductDetails = await _productService.GetProductByIdAsync(Id);
                return Page();
            }

            try
            {
                await _productService.UpdateProductAsync(Id, Product);
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
            Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();

            var suppliers = await _supplierService.GetSuppliersAsync(new DAL.DTOs.Supplier.SupplierQueryParamsDTO { PageNumber = 1, PageSize = 100 });
            Suppliers = suppliers.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
        }
    }
}

