using DAL.DTOs.Product;
using ProductImageModel = DAL.Models.ProductImage;
using ProductModel = DAL.Models.Product;

namespace DAL.Repositories.Product.Interface
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductModel>> GetAllAsync(ProductQueryParamsDTO queryParams);
        Task<int> CountAsync(ProductQueryParamsDTO queryParams);
        Task<ProductModel> GetByIdAsync(int id, bool includeInactive = false);
        Task<ProductModel> GetBySkuAsync(string sku, bool includeInactive = false);
        Task<ProductModel> CreateAsync(ProductModel product);
        Task UpdateAsync(ProductModel product);
        Task DeleteAsync(int id);
        Task RestoreAsync(int id);
        Task<bool> ExistsAsync(int id, bool includeInactive = false);
        Task<ProductImageModel> GetImageByIdAsync(int imageId);
        Task AddImageAsync(ProductImageModel image);
        Task DeleteImageAsync(ProductImageModel image);
    }
}
