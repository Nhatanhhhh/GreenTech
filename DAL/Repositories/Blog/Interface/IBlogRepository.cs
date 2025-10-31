using DAL.DTOs.Blog;
using BlogModel = DAL.Models.Blog;

namespace DAL.Repositories.Blog.Interface
{
    public interface IBlogRepository
    {
        Task<IEnumerable<BlogModel>> GetAllAsync(BlogQueryParamsDTO queryParams);
        Task<int> CountAsync(BlogQueryParamsDTO queryParams);
        Task<BlogModel> GetByIdAsync(int id);
        Task<BlogModel> GetBySlugAsync(string slug);
        Task<BlogModel> CreateAsync(BlogModel blog);
        Task UpdateAsync(BlogModel blog);
        Task DeleteAsync(int id);
    }
}
