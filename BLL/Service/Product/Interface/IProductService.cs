using DAL.DTOs.Product;
using DAL.DTOs.ProductImage;
using Microsoft.AspNetCore.Http;

namespace BLL.Service.Product.Interface
{
    public interface IProductService
    {
        Task<ProductResponseDTO> CreateProductAsync(
            CreateProductDTO createProductDto,
            IFormFile? mainImage = null,
            List<IFormFile>? additionalImages = null
        );
        Task<ProductResponseDTO> UpdateProductAsync(
            int productId,
            UpdateProductDTO updateProductDto,
            IFormFile? mainImage = null,
            List<IFormFile>? additionalImages = null
        );
        Task DeleteProductAsync(int productId);
        Task<IEnumerable<ProductResponseDTO>> GetProductsAsync(ProductQueryParamsDTO queryParams);
        Task<int> GetProductsCountAsync(ProductQueryParamsDTO queryParams);
        Task<ProductResponseDTO> GetProductByIdAsync(int productId, bool includeInactive = false);
        Task<List<ProductImageDTO>> UploadImagesAsync(int productId, List<IFormFile> files);
        Task DeleteImageAsync(int productId, int imageId);
    }
}
