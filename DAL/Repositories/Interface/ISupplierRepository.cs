using DAL.DTOs.Supplier;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface ISupplierRepository
    {
        Task<IEnumerable<Supplier>> GetAllAsync(SupplierQueryParamsDTO queryParams);
        Task<int> CountAsync(SupplierQueryParamsDTO queryParams);
        Task<Supplier> GetByIdAsync(int id);
        Task<Supplier> GetByCodeAsync(string code);
        Task<Supplier> CreateAsync(Supplier supplier);
        Task UpdateAsync(Supplier supplier);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
