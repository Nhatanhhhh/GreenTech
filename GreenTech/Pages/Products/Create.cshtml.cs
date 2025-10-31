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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return Page();
            }

            await _productService.CreateProductAsync(Product);
            return RedirectToPage("./Index");
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
