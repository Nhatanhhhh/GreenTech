using BLL.Service.Interface;
using DAL.DTOs.Supplier;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Suppliers
{
    [AdminAuthorize]
    public class IndexModel : PageModel
    {
        private readonly ISupplierService _supplierService;

        public IndexModel(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        public IEnumerable<SupplierDTO> Suppliers { get; set; } = new List<SupplierDTO>();
        public SupplierQueryParamsDTO QueryParams { get; set; } = new SupplierQueryParamsDTO();
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / QueryParams.PageSize);

        public async Task OnGetAsync(SupplierQueryParamsDTO queryParams)
        {
            QueryParams = queryParams;
            if (queryParams.PageNumber <= 0)
            {
                QueryParams.PageNumber = 1;
            }

            if (queryParams.PageSize <= 0)
            {
                QueryParams.PageSize = 10;
            }

            Suppliers = await _supplierService.GetSuppliersAsync(QueryParams);
            TotalCount = await _supplierService.GetSuppliersCountAsync(QueryParams);
        }
    }
}
