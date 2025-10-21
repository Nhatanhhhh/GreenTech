using DAL.DTOs.Product;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(ProductQueryParamsDTO queryParams);
        Task<int> CountAsync(ProductQueryParamsDTO queryParams);
        Task<Product> GetByIdAsync(int id);
        Task<Product> GetBySkuAsync(string sku);
        Task<Product> CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<ProductImage> GetImageByIdAsync(int imageId);
        Task AddImageAsync(ProductImage image);
        Task DeleteImageAsync(ProductImage image);
    }
}
