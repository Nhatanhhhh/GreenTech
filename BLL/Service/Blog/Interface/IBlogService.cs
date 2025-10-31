using DAL.DTOs.Blog;
using Microsoft.AspNetCore.Http;

namespace BLL.Service.Blog.Interface
{
    public interface IBlogService
    {
        Task<BlogResponseDTO> CreateBlogAsync(CreateBlogDTO createBlogDto, int authorId);
        Task<BlogResponseDTO> UpdateBlogAsync(int blogId, UpdateBlogDTO updateBlogDto);
        Task DeleteBlogAsync(int blogId);
        Task<IEnumerable<BlogResponseDTO>> GetBlogsAsync(BlogQueryParamsDTO queryParams);
        Task<int> GetBlogsCountAsync(BlogQueryParamsDTO queryParams);
        Task<BlogResponseDTO> GetBlogByIdAsync(int blogId);
        Task<BlogResponseDTO> GetBlogBySlugAsync(string slug);
        Task<string> UpdateFeaturedImageAsync(int blogId, IFormFile imageFile);
    }
}
