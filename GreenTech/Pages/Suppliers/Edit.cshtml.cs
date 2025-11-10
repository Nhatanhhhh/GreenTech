using BLL.Service.Supplier.Interface;
using DAL.DTOs.Supplier;
using GreenTech.Filters;
using GreenTech.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace GreenTech.Pages.Suppliers
{
    [AdminAuthorize]
    public class EditModel : PageModel
    {
        private readonly ISupplierService _supplierService;
        private readonly IHubContext<SupplierHub> _supplierHubContext;

        public EditModel(ISupplierService supplierService, IHubContext<SupplierHub> supplierHubContext)
        {
            _supplierService = supplierService;
            _supplierHubContext = supplierHubContext;
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
                var updated = await _supplierService.UpdateSupplierAsync(id, Supplier);
                
                // Gửi SignalR notification đến tất cả admin đang kết nối
                await _supplierHubContext.Clients.All.SendAsync("SupplierChanged", new
                {
                    action = "updated",
                    supplierId = updated.Id,
                    name = updated.Name,
                    code = updated.Code,
                    contactPerson = updated.ContactPerson,
                    phone = updated.Phone,
                    email = updated.Email,
                    address = updated.Address,
                    taxCode = updated.TaxCode,
                    paymentTerms = updated.PaymentTerms,
                    isActive = updated.IsActive,
                    updatedAt = updated.UpdatedAt
                });
                
                return RedirectToPage("./Index");
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật nhà cung cấp.");
                return Page();
            }
        }
    }
}
