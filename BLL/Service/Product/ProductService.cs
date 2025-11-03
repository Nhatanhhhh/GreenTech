using System.Text.RegularExpressions;
using BLL.Service.Cloudinary.Interface;
using BLL.Service.Product.Interface;
using DAL.DTOs.Product;
using DAL.DTOs.ProductImage;
using DAL.Models;
using DAL.Repositories.Product.Interface;
using DAL.Utils.AutoMapper;
using Microsoft.AspNetCore.Http;

namespace BLL.Service.Product
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IFileStorageService _fileStorageService;

        public ProductService(
            IProductRepository productRepository,
            IFileStorageService fileStorageService
        )
        {
            _productRepository = productRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<ProductResponseDTO> CreateProductAsync(CreateProductDTO createProductDto)
        {
            if (await _productRepository.GetBySkuAsync(createProductDto.Sku) != null)
            {
                throw new ArgumentException(
                    $"Product with SKU '{createProductDto.Sku}' already exists."
                );
            }

            var product = AutoMapper.ToProduct(createProductDto);
            product.Slug = GenerateSlug(product.Name); // Generate slug from name

            var createdProduct = await _productRepository.CreateAsync(product);
            return AutoMapper.ToProductResponseDTO(createdProduct);
        }

        public async Task DeleteProductAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId, includeInactive: true);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            // Soft delete: không xóa images vì product vẫn tồn tại, chỉ inactive
            // Chỉ soft delete bằng cách set IsActive = false
            await _productRepository.DeleteAsync(productId);
        }

        public async Task RestoreProductAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId, includeInactive: true);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            if (product.IsActive)
            {
                throw new InvalidOperationException("Product is already active.");
            }

            await _productRepository.RestoreAsync(productId);
        }

        public async Task DeleteImageAsync(int productId, int imageId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            var image = await _productRepository.GetImageByIdAsync(imageId);
            if (image == null || image.ProductId != productId)
            {
                throw new KeyNotFoundException(
                    $"Image with ID {imageId} not found for this product."
                );
            }

            await _fileStorageService.DeleteFileAsync(image.ImageUrl);
            await _productRepository.DeleteImageAsync(image);

            // If the deleted image was the main image, find a new one
            if (product.Image == image.ImageUrl)
            {
                // Refresh product data to get latest image list
                var updatedProduct = await _productRepository.GetByIdAsync(productId);
                var nextImage = updatedProduct.ProductImages.FirstOrDefault();
                updatedProduct.Image = nextImage?.ImageUrl; // Can be null if no other images
                await _productRepository.UpdateAsync(updatedProduct);
            }
        }

        public async Task<ProductResponseDTO> GetProductByIdAsync(
            int productId,
            bool includeInactive = false
        )
        {
            var product = await _productRepository.GetByIdAsync(productId, includeInactive);
            if (product == null)
            {
                return null;
            }
            return AutoMapper.ToProductResponseDTO(product);
        }

        public async Task<IEnumerable<ProductResponseDTO>> GetProductsAsync(
            ProductQueryParamsDTO queryParams
        )
        {
            var products = await _productRepository.GetAllAsync(queryParams);
            return AutoMapper.ToProductResponseDTOs(products);
        }

        public async Task<int> GetProductsCountAsync(ProductQueryParamsDTO queryParams)
        {
            return await _productRepository.CountAsync(queryParams);
        }

        public async Task<ProductResponseDTO> UpdateProductAsync(
            int productId,
            UpdateProductDTO updateProductDto
        )
        {
            // Use includeInactive: true để có thể update cả products đã inactive
            var existingProduct = await _productRepository.GetByIdAsync(
                productId,
                includeInactive: true
            );
            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            AutoMapper.ApplyUpdatesToProduct(updateProductDto, existingProduct);
            existingProduct.Slug = GenerateSlug(updateProductDto.Name); // Regenerate slug if name changes

            await _productRepository.UpdateAsync(existingProduct);
            return AutoMapper.ToProductResponseDTO(existingProduct);
        }

        public async Task<List<ProductImageDTO>> UploadImagesAsync(
            int productId,
            List<IFormFile> files
        )
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            var uploadedImages = new List<ProductImageDTO>();
            bool productHasNoMainImage = string.IsNullOrEmpty(product.Image);

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var imageUrl = await _fileStorageService.SaveFileAsync(file, "products");

                // The first uploaded image becomes the main image only if the product doesn't already have one.
                if (i == 0 && productHasNoMainImage)
                {
                    product.Image = imageUrl;
                    await _productRepository.UpdateAsync(product);
                }

                var productImage = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = imageUrl,
                    AltText = $"{product.Name} - image {product.ProductImages.Count + i + 1}",
                    IsPrimary = false, // You might add logic to set one as primary
                    CreatedAt = DateTime.UtcNow,
                };
                await _productRepository.AddImageAsync(productImage);
                uploadedImages.Add(AutoMapper.ToProductImageDTO(productImage));
            }

            return uploadedImages;
        }

        private static string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower().Trim();
            str = Regex.Replace(str, @"[àáạảãâầấậẩẫăằắặẳẵ]", "a");
            str = Regex.Replace(str, @"[èéẹẻẽêềếệểễ]", "e");
            str = Regex.Replace(str, @"[ìíịỉĩ]", "i");
            str = Regex.Replace(str, @"[òóọỏõôồốộổỗơờớợởỡ]", "o");
            str = Regex.Replace(str, @"[ùúụủũưừứựửữ]", "u");
            str = Regex.Replace(str, @"[ỳýỵỷỹ]", "y");
            str = Regex.Replace(str, @"[đ]", "d");
            str = Regex.Replace(str, @"[^a-z0-9\s-]", ""); // remove invalid chars
            str = Regex.Replace(str, @"\s+", " ").Trim(); // convert multiple spaces into one space
            str = Regex.Replace(str, @"\s", "-"); // replace spaces with hyphens
            return str;
        }
    }
}
