using DAL.Context;
using DAL.DTOs.Banner;
using DAL.Models;
using DAL.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class BannerRepository : IBannerRepository
    {
        private readonly AppDbContext _context;

        public BannerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Banner> GetByIdAsync(int id, bool includeRelations = false)
        {
            var query = _context.Banners.AsQueryable();

            if (includeRelations)
            {
                query = query.Include(b => b.Creator);
            }

            return await query.FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Banner>> GetAllAsync()
        {
            return await _context.Banners
                .Include(b => b.Creator)
                .OrderBy(b => b.Position)
                .ThenBy(b => b.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Banner>> GetWithQueryAsync(BannerQueryParams queryParams)
        {
            var query = _context.Banners
                .Include(b => b.Creator)
                .AsQueryable();

            // Filter by search keyword
            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                var searchLower = queryParams.Search.ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(searchLower) ||
                    b.Description.ToLower().Contains(searchLower));
            }

            // Filter by active status
            if (queryParams.IsActive.HasValue)
            {
                query = query.Where(b => b.IsActive == queryParams.IsActive.Value);
            }

            // Filter by position
            if (!string.IsNullOrEmpty(queryParams.Position))
            {
                query = query.Where(b => b.Position.ToString() == queryParams.Position);
            }

            // Filter by start date range
            if (queryParams.StartDateFrom.HasValue)
            {
                query = query.Where(b => b.StartDate >= queryParams.StartDateFrom.Value);
            }

            if (queryParams.StartDateTo.HasValue)
            {
                query = query.Where(b => b.StartDate <= queryParams.StartDateTo.Value);
            }

            // Sorting
            query = queryParams.SortBy?.ToLower() switch
            {
                "title" => queryParams.SortDescending
                    ? query.OrderByDescending(b => b.Title)
                    : query.OrderBy(b => b.Title),
                "position" => queryParams.SortDescending
                    ? query.OrderByDescending(b => b.Position).ThenByDescending(b => b.SortOrder)
                    : query.OrderBy(b => b.Position).ThenBy(b => b.SortOrder),
                "sortorder" => queryParams.SortDescending
                    ? query.OrderByDescending(b => b.SortOrder)
                    : query.OrderBy(b => b.SortOrder),
                "createdat" => queryParams.SortDescending
                    ? query.OrderByDescending(b => b.CreatedAt)
                    : query.OrderBy(b => b.CreatedAt),
                "clickcount" => queryParams.SortDescending
                    ? query.OrderByDescending(b => b.ClickCount)
                    : query.OrderBy(b => b.ClickCount),
                _ => query.OrderBy(b => b.Position).ThenBy(b => b.SortOrder)
            };

            // Pagination
            if (queryParams.Page > 0 && queryParams.PageSize > 0)
            {
                query = query
                    .Skip((queryParams.Page - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize);
            }

            return await query.ToListAsync();
        }

        public async Task<Banner> CreateAsync(Banner banner)
        {
            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();

            // Load creator information
            await _context.Entry(banner)
                .Reference(b => b.Creator)
                .LoadAsync();

            return banner;
        }

        public async Task<Banner> UpdateAsync(Banner banner)
        {
            var existingBanner = await _context.Banners.FindAsync(banner.Id);
            if (existingBanner == null) return null;

            existingBanner.Title = banner.Title;
            existingBanner.Description = banner.Description;
            existingBanner.ImageUrl = banner.ImageUrl;
            existingBanner.LinkUrl = banner.LinkUrl;
            existingBanner.Position = banner.Position;
            existingBanner.SortOrder = banner.SortOrder;
            existingBanner.IsActive = banner.IsActive;
            existingBanner.StartDate = banner.StartDate;
            existingBanner.EndDate = banner.EndDate;
            existingBanner.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Load creator information
            await _context.Entry(existingBanner)
                .Reference(b => b.Creator)
                .LoadAsync();

            return existingBanner;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return false;

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Banners.AnyAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Banner>> GetActiveBannersAsync()
        {
            var now = DateTime.Now;

            return await _context.Banners
                .Include(b => b.Creator)
                .Where(b => b.IsActive &&
                           (b.StartDate == null || b.StartDate <= now) &&
                           (b.EndDate == null || b.EndDate >= now))
                .OrderBy(b => b.Position)
                .ThenBy(b => b.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Banner>> GetBannersByPositionAsync(string position)
        {
            return await _context.Banners
                .Include(b => b.Creator)
                .Where(b => b.Position.ToString() == position)
                .OrderBy(b => b.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Banner>> GetActiveBannersByPositionAsync(string position)
        {
            var now = DateTime.Now;

            return await _context.Banners
                .Include(b => b.Creator)
                .Where(b => b.Position.ToString() == position &&
                           b.IsActive &&
                           (b.StartDate == null || b.StartDate <= now) &&
                           (b.EndDate == null || b.EndDate >= now))
                .OrderBy(b => b.SortOrder)
                .ToListAsync();
        }

        public async Task<int> GetMaxSortOrderAsync(string position)
        {
            var maxSort = await _context.Banners
                .Where(b => b.Position.ToString() == position)
                .MaxAsync(b => (int?)b.SortOrder);

            return maxSort ?? 0;
        }

        public async Task<bool> IncrementClickCountAsync(int bannerId)
        {
            var banner = await _context.Banners.FindAsync(bannerId);
            if (banner == null) return false;

            banner.ClickCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSortOrdersAsync(Dictionary<int, int> bannerSortOrders)
        {
            foreach (var kvp in bannerSortOrders)
            {
                var banner = await _context.Banners.FindAsync(kvp.Key);
                if (banner != null)
                {
                    banner.SortOrder = kvp.Value;
                    banner.UpdatedAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
