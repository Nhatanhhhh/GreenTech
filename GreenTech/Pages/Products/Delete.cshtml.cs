using BLL.Service.Interface;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Products
{
    [AdminAuthorize]
    public class DeleteModel : PageModel
    {
        private readonly IProductService _productService;

        public DeleteModel(IProductService productService)
        {
            _productService = productService;
        }

        public DAL.DTOs.Product.ProductResponseDTO? Product { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Product = await _productService.GetProductByIdAsync(Id);
            if (Product == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await _productService.DeleteProductAsync(Id);
                return RedirectToPage("./Index");
            }
            catch (KeyNotFoundException)
            {
                ModelState.AddModelError("", "Không tìm thấy sản phẩm để xóa.");
                Product = await _productService.GetProductByIdAsync(Id);
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi xóa sản phẩm: {ex.Message}");
                Product = await _productService.GetProductByIdAsync(Id);
                return Page();
            }
        }
    }
}

