using BLL.Service.Supplier.Interface;
using DAL.DTOs.Supplier;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Suppliers
{
    [AdminAuthorize]
    public class DeleteModel : PageModel
    {
        private readonly ISupplierService _supplierService;

        public DeleteModel(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        public SupplierDTO Supplier { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Supplier = await _supplierService.GetSupplierByIdAsync(id);
            if (Supplier == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                await _supplierService.DeleteSupplierAsync(id);
                return RedirectToPage("./Index");
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                Supplier = await _supplierService.GetSupplierByIdAsync(id);
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi xóa nhà cung cấp.");
                Supplier = await _supplierService.GetSupplierByIdAsync(id);
                return Page();
            }
        }
    }
}
