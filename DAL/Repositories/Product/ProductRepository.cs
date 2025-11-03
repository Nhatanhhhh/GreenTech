using DAL.Context;
using DAL.DTOs.Product;
using DAL.Repositories.Product.Interface;
using Microsoft.EntityFrameworkCore;
using ProductImageModel = DAL.Models.ProductImage;
using ProductModel = DAL.Models.Product;

namespace DAL.Repositories.Product
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductModel> CreateAsync(ProductModel product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task DeleteAsync(int id)
        {
            var product = await GetByIdAsync(id, includeInactive: true);
            if (product != null && product.IsActive)
            {
                // Soft delete: set IsActive = false instead of removing from database
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RestoreAsync(int id)
        {
            var product = await GetByIdAsync(id, includeInactive: true);
            if (product != null && !product.IsActive)
            {
                // Restore: set IsActive = true
                product.IsActive = true;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ProductModel>> GetAllAsync(ProductQueryParamsDTO queryParams)
        {
            var query = _context
                .Products.Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive) // Only get active products
                .AsQueryable();

            query = ApplyFilters(query, queryParams);
            query = ApplySorting(query, queryParams);

            return await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();
        }

        public async Task<int> CountAsync(ProductQueryParamsDTO queryParams)
        {
            var query = _context
                .Products.Where(p => p.IsActive) // Only count active products
                .AsQueryable();

            query = ApplyFilters(query, queryParams);
            return await query.CountAsync();
        }

        public async Task<ProductModel> GetByIdAsync(int id, bool includeInactive = false)
        {
            var query = _context
                .Products.Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.ProductImages)
                .Where(p => p.Id == id);

            // Only filter by IsActive if we don't want to include inactive items
            if (!includeInactive)
            {
                query = query.Where(p => p.IsActive);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<ProductModel> GetBySkuAsync(string sku, bool includeInactive = false)
        {
            var query = _context.Products.Where(p => p.Sku == sku);

            // Only filter by IsActive if we don't want to include inactive items
            if (!includeInactive)
            {
                query = query.Where(p => p.IsActive);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(ProductModel product)
        {
            // Get existing product to check current IsActive status
            var existing = await _context.Products.FindAsync(product.Id);
            if (existing != null)
            {
                // Update logic:
                // - Update KHÔNG được phép chuyển từ Active -> Inactive (chỉ có Delete mới làm được)
                // - Update CÓ THỂ restore từ Inactive -> Active

                // Update all properties
                existing.Name = product.Name;
                existing.Description = product.Description;
                existing.ShortDescription = product.ShortDescription;
                existing.CategoryId = product.CategoryId;
                existing.SupplierId = product.SupplierId;
                existing.CostPrice = product.CostPrice;
                existing.SellPrice = product.SellPrice;
                existing.Quantity = product.Quantity;
                existing.CareInstructions = product.CareInstructions;
                existing.PlantSize = product.PlantSize;
                existing.Weight = product.Weight;
                existing.Dimensions = product.Dimensions;
                existing.Tags = product.Tags;
                existing.PointsEarned = product.PointsEarned;
                existing.IsFeatured = product.IsFeatured;
                existing.SeoTitle = product.SeoTitle;
                existing.SeoDescription = product.SeoDescription;

                // IsActive: chỉ cho phép restore (Inactive -> Active), không cho phép deactivate (Active -> Inactive)
                if (!existing.IsActive && product.IsActive)
                {
                    // Restore: cho phép chuyển từ Inactive -> Active
                    existing.IsActive = true;
                }
                // Nếu đang Active và cố set thành Inactive, giữ nguyên Active (không cho phép)

                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else
            {
                // If not found, ensure IsActive = true for new products
                if (!product.IsActive)
                {
                    product.IsActive = true;
                }
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id, bool includeInactive = false)
        {
            var query = _context.Products.Where(p => p.Id == id);

            // Only filter by IsActive if we don't want to include inactive items
            if (!includeInactive)
            {
                query = query.Where(p => p.IsActive);
            }

            return await query.AnyAsync();
        }

        public async Task AddImageAsync(ProductImageModel image)
        {
            await _context.ProductImages.AddAsync(image);
            await _context.SaveChangesAsync();
        }

        public async Task<ProductImageModel> GetImageByIdAsync(int imageId)
        {
            return await _context.ProductImages.FindAsync(imageId);
        }

        public async Task DeleteImageAsync(ProductImageModel image)
        {
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();
        }

        private IQueryable<ProductModel> ApplyFilters(
            IQueryable<ProductModel> query,
            ProductQueryParamsDTO queryParams
        )
        {
            // Note: IsDeleted filter is already applied in GetAllAsync, so we don't need to filter again here
            // unless we want to allow querying deleted products with a special parameter (not implemented for now)

            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                var searchTermLower = queryParams.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTermLower)
                    || p.Sku.ToLower().Contains(searchTermLower)
                );
            }
            if (queryParams.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == queryParams.CategoryId.Value);
            }
            if (queryParams.SupplierId.HasValue)
            {
                query = query.Where(p => p.SupplierId == queryParams.SupplierId.Value);
            }
            if (queryParams.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == queryParams.IsActive.Value);
            }
            if (queryParams.IsFeatured.HasValue)
            {
                query = query.Where(p => p.IsFeatured == queryParams.IsFeatured.Value);
            }
            return query;
        }

        private IQueryable<ProductModel> ApplySorting(
            IQueryable<ProductModel> query,
            ProductQueryParamsDTO queryParams
        )
        {
            var isDescending = queryParams.SortOrder?.ToLower() == "desc";

            switch (queryParams.SortBy?.ToLower())
            {
                case "name":
                    query = isDescending
                        ? query.OrderByDescending(p => p.Name)
                        : query.OrderBy(p => p.Name);
                    break;
                case "price":
                    query = isDescending
                        ? query.OrderByDescending(p => p.SellPrice)
                        : query.OrderBy(p => p.SellPrice);
                    break;
                case "createdat":
                    query = isDescending
                        ? query.OrderByDescending(p => p.CreatedAt)
                        : query.OrderBy(p => p.CreatedAt);
                    break;
                default:
                    query = query.OrderBy(p => p.Id);
                    break;
            }
            return query;
        }
    }
}
