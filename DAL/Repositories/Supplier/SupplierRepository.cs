using DAL.Context;
using DAL.DTOs.Supplier;
using DAL.Repositories.Supplier.Interface;
using Microsoft.EntityFrameworkCore;
using SupplierModel = DAL.Models.Supplier;

namespace DAL.Repositories.Supplier
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly AppDbContext _context;

        public SupplierRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SupplierModel> CreateAsync(SupplierModel supplier)
        {
            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        public async Task DeleteAsync(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier != null)
            {
                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<SupplierModel>> GetAllAsync(
            SupplierQueryParamsDTO queryParams
        )
        {
            var query = _context.Suppliers.AsQueryable();

            query = ApplyFilters(query, queryParams);
            query = ApplySorting(query, queryParams);

            return await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();
        }

        public async Task<int> CountAsync(SupplierQueryParamsDTO queryParams)
        {
            var query = _context.Suppliers.AsQueryable();
            query = ApplyFilters(query, queryParams);
            return await query.CountAsync();
        }

        public async Task<SupplierModel> GetByIdAsync(int id)
        {
            return await _context.Suppliers.FindAsync(id);
        }

        public async Task<SupplierModel> GetByCodeAsync(string code)
        {
            return await _context.Suppliers.FirstOrDefaultAsync(s =>
                s.Code.ToLower() == code.ToLower()
            );
        }

        public async Task UpdateAsync(SupplierModel supplier)
        {
            _context.Entry(supplier).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Suppliers.AnyAsync(s => s.Id == id);
        }

        private IQueryable<SupplierModel> ApplyFilters(
            IQueryable<SupplierModel> query,
            SupplierQueryParamsDTO queryParams
        )
        {
            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                var searchTermLower = queryParams.SearchTerm.ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(searchTermLower)
                    || s.Code.ToLower().Contains(searchTermLower)
                    || s.Email.ToLower().Contains(searchTermLower)
                );
            }
            if (queryParams.IsActive.HasValue)
            {
                query = query.Where(s => s.IsActive == queryParams.IsActive.Value);
            }
            return query;
        }

        private IQueryable<SupplierModel> ApplySorting(
            IQueryable<SupplierModel> query,
            SupplierQueryParamsDTO queryParams
        )
        {
            var isDescending = queryParams.SortOrder?.ToLower() == "desc";

            switch (queryParams.SortBy?.ToLower())
            {
                case "name":
                    query = isDescending
                        ? query.OrderByDescending(s => s.Name)
                        : query.OrderBy(s => s.Name);
                    break;
                case "code":
                    query = isDescending
                        ? query.OrderByDescending(s => s.Code)
                        : query.OrderBy(s => s.Code);
                    break;
                case "createdat":
                    query = isDescending
                        ? query.OrderByDescending(s => s.CreatedAt)
                        : query.OrderBy(s => s.CreatedAt);
                    break;
                default:
                    query = query.OrderBy(s => s.Id);
                    break;
            }
            return query;
        }
    }
}
