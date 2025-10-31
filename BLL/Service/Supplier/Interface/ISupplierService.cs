using DAL.DTOs.Supplier;

namespace BLL.Service.Supplier.Interface
{
    public interface ISupplierService
    {
        Task<SupplierDTO> CreateSupplierAsync(CreateSupplierDTO createSupplierDto);
        Task<SupplierDTO> UpdateSupplierAsync(int supplierId, UpdateSupplierDTO updateSupplierDto);
        Task DeleteSupplierAsync(int supplierId);
        Task<IEnumerable<SupplierDTO>> GetSuppliersAsync(SupplierQueryParamsDTO queryParams);
        Task<int> GetSuppliersCountAsync(SupplierQueryParamsDTO queryParams);
        Task<SupplierDTO> GetSupplierByIdAsync(int supplierId);
    }
}
