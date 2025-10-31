using DAL.Context;
using DAL.DTOs.Blog;
using DAL.Repositories.Blog.Interface;
using Microsoft.EntityFrameworkCore;
using BlogModel = DAL.Models.Blog;

namespace DAL.Repositories.Blog
{
    public class BlogRepository : IBlogRepository
    {
        private readonly AppDbContext _context;

        public BlogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BlogModel>> GetAllAsync(BlogQueryParamsDTO queryParams)
        {
            var query = _context
                .Blogs.Include(b => b.Author)
                .Include(b => b.Category)
                .AsQueryable();

            query = ApplyFilters(query, queryParams);
            query = ApplySorting(query, queryParams);

            return await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();
        }

        public async Task<int> CountAsync(BlogQueryParamsDTO queryParams)
        {
            var query = _context.Blogs.AsQueryable();
            query = ApplyFilters(query, queryParams);
            return await query.CountAsync();
        }

        public async Task<BlogModel> GetByIdAsync(int id)
        {
            return await _context
                .Blogs.Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<BlogModel> GetBySlugAsync(string slug)
        {
            return await _context
                .Blogs.Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Slug == slug);
        }

        public async Task<BlogModel> CreateAsync(BlogModel blog)
        {
            await _context.Blogs.AddAsync(blog);
            await _context.SaveChangesAsync();
            return blog;
        }

        public async Task UpdateAsync(BlogModel blog)
        {
            _context.Blogs.Update(blog);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();
            }
        }

        private IQueryable<BlogModel> ApplyFilters(
            IQueryable<BlogModel> query,
            BlogQueryParamsDTO queryParams
        )
        {
            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                var searchTermLower = queryParams.SearchTerm.ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(searchTermLower)
                    || b.Content.ToLower().Contains(searchTermLower)
                );
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Tag))
            {
                var tagLower = queryParams.Tag.ToLower();
                query = query.Where(b => b.Tags.ToLower().Contains(tagLower));
            }

            if (queryParams.CategoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == queryParams.CategoryId.Value);
            }

            if (queryParams.AuthorId.HasValue)
            {
                query = query.Where(b => b.AuthorId == queryParams.AuthorId.Value);
            }

            if (queryParams.IsPublished.HasValue)
            {
                query = query.Where(b => b.IsPublished == queryParams.IsPublished.Value);
            }

            if (queryParams.IsFeatured.HasValue)
            {
                query = query.Where(b => b.IsFeatured == queryParams.IsFeatured.Value);
            }

            return query;
        }

        private IQueryable<BlogModel> ApplySorting(
            IQueryable<BlogModel> query,
            BlogQueryParamsDTO queryParams
        )
        {
            var isDescending = queryParams.SortOrder?.ToLower() == "desc";

            switch (queryParams.SortBy?.ToLower())
            {
                case "title":
                    query = isDescending
                        ? query.OrderByDescending(b => b.Title)
                        : query.OrderBy(b => b.Title);
                    break;
                case "publishedat":
                    query = isDescending
                        ? query.OrderByDescending(b => b.PublishedAt)
                        : query.OrderBy(b => b.PublishedAt);
                    break;
                case "viewcount":
                    query = isDescending
                        ? query.OrderByDescending(b => b.ViewCount)
                        : query.OrderBy(b => b.ViewCount);
                    break;
                case "createdat":
                default:
                    query = isDescending
                        ? query.OrderByDescending(b => b.CreatedAt)
                        : query.OrderBy(b => b.CreatedAt);
                    break;
            }
            return query;
        }
    }
}
