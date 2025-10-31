using DAL.DTOs.Supplier;
using SupplierModel = DAL.Models.Supplier;

namespace DAL.Repositories.Supplier.Interface
{
    public interface ISupplierRepository
    {
        Task<IEnumerable<SupplierModel>> GetAllAsync(SupplierQueryParamsDTO queryParams);
        Task<int> CountAsync(SupplierQueryParamsDTO queryParams);
        Task<SupplierModel> GetByIdAsync(int id);
        Task<SupplierModel> GetByCodeAsync(string code);
        Task<SupplierModel> CreateAsync(SupplierModel supplier);
        Task UpdateAsync(SupplierModel supplier);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
