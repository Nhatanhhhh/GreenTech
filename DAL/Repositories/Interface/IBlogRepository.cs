using DAL.DTOs.Blog;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface IBlogRepository
    {
        Task<IEnumerable<Blog>> GetAllAsync(BlogQueryParamsDTO queryParams);
        Task<int> CountAsync(BlogQueryParamsDTO queryParams);
        Task<Blog> GetByIdAsync(int id);
        Task<Blog> GetBySlugAsync(string slug);
        Task<Blog> CreateAsync(Blog blog);
        Task UpdateAsync(Blog blog);
        Task DeleteAsync(int id);
    }
}
