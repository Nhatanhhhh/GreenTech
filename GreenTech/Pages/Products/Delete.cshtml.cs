using BLL.Service.Product.Interface;
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

        public async Task<IActionResult> OnPostDeleteAsync()
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
                Product = await _productService.GetProductByIdAsync(Id);
                ModelState.AddModelError(
                    string.Empty,
                    "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
                );
                return Page();
            }

            if (string.IsNullOrEmpty(userRoles) || !userRoles.Contains("ROLE_ADMIN"))
            {
                Product = await _productService.GetProductByIdAsync(Id);
                ModelState.AddModelError(
                    string.Empty,
                    "Bạn không có quyền thực hiện thao tác này."
                );
                return Page();
            }

            try
            {
                await _productService.DeleteProductAsync(Id);
                TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
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
