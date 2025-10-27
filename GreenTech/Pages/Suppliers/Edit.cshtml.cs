using BLL.Service.Interface;
using DAL.DTOs.Supplier;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Suppliers
{
    [AdminAuthorize]
    public class EditModel : PageModel
    {
        private readonly ISupplierService _supplierService;

        public EditModel(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [BindProperty]
        public UpdateSupplierDTO Supplier { get; set; }

        public SupplierDTO SupplierInfo { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            SupplierInfo = await _supplierService.GetSupplierByIdAsync(id);
            if (SupplierInfo == null)
            {
                return NotFound();
            }

            Supplier = new UpdateSupplierDTO
            {
                Name = SupplierInfo.Name,
                ContactPerson = SupplierInfo.ContactPerson,
                Phone = SupplierInfo.Phone,
                Email = SupplierInfo.Email,
                Address = SupplierInfo.Address,
                TaxCode = SupplierInfo.TaxCode,
                PaymentTerms = SupplierInfo.PaymentTerms,
                IsActive = SupplierInfo.IsActive,
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                SupplierInfo = await _supplierService.GetSupplierByIdAsync(id);
                return Page();
            }

            try
            {
                await _supplierService.UpdateSupplierAsync(id, Supplier);
                return RedirectToPage("./Index");
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật nhà cung cấp.");
                return Page();
            }
        }
    }
}
