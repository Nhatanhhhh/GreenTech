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
            var product = await GetByIdAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ProductModel>> GetAllAsync(ProductQueryParamsDTO queryParams)
        {
            var query = _context
                .Products.Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.ProductImages)
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
            var query = _context.Products.AsQueryable();
            query = ApplyFilters(query, queryParams);
            return await query.CountAsync();
        }

        public async Task<ProductModel> GetByIdAsync(int id)
        {
            return await _context
                .Products.Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<ProductModel> GetBySkuAsync(string sku)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Sku == sku);
        }

        public async Task UpdateAsync(ProductModel product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
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
