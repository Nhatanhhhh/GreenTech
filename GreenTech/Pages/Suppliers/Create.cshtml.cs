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
    public class CreateModel : PageModel
    {
        private readonly ISupplierService _supplierService;
        private readonly IHubContext<SupplierHub> _supplierHubContext;

        public CreateModel(ISupplierService supplierService, IHubContext<SupplierHub> supplierHubContext)
        {
            _supplierService = supplierService;
            _supplierHubContext = supplierHubContext;
        }

        [BindProperty]
        public CreateSupplierDTO Supplier { get; set; } = new CreateSupplierDTO();

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var created = await _supplierService.CreateSupplierAsync(Supplier);
                
                // Gửi SignalR notification đến tất cả admin đang kết nối
                await _supplierHubContext.Clients.All.SendAsync("SupplierChanged", new
                {
                    action = "created",
                    supplierId = created.Id,
                    name = created.Name,
                    code = created.Code,
                    contactPerson = created.ContactPerson,
                    phone = created.Phone,
                    email = created.Email,
                    address = created.Address,
                    taxCode = created.TaxCode,
                    paymentTerms = created.PaymentTerms,
                    isActive = created.IsActive,
                    createdAt = created.CreatedAt
                });
                
                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi tạo nhà cung cấp.");
                return Page();
            }
        }
    }
}
