using BLL.Service.Interface;
using DAL.DTOs.Blog;
using DAL.Repositories.Interface;
using DAL.Utils.AutoMapper;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace BLL.Service
{
    public class BlogService : IBlogService
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IFileStorageService _fileStorageService;

        public BlogService(IBlogRepository blogRepository, IFileStorageService fileStorageService)
        {
            _blogRepository = blogRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<BlogResponseDTO> CreateBlogAsync(CreateBlogDTO createBlogDto, int authorId)
        {
            var blog = AutoMapper.ToBlog(createBlogDto, authorId);
            blog.Slug = await GenerateUniqueSlug(blog.Title);

            if (blog.IsPublished)
            {
                blog.PublishedAt = DateTime.UtcNow;
            }

            var createdBlog = await _blogRepository.CreateAsync(blog);
            return AutoMapper.ToBlogResponseDTO(createdBlog);
        }

        public async Task DeleteBlogAsync(int blogId)
        {
            var blog = await _blogRepository.GetByIdAsync(blogId);
            if (blog == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy bài viết với ID {blogId}.");
            }

            if (!string.IsNullOrEmpty(blog.FeaturedImage))
            {
                await _fileStorageService.DeleteFileAsync(blog.FeaturedImage);
            }

            await _blogRepository.DeleteAsync(blogId);
        }

        public async Task<BlogResponseDTO> GetBlogByIdAsync(int blogId)
        {
            var blog = await _blogRepository.GetByIdAsync(blogId);
            return blog == null ? null : AutoMapper.ToBlogResponseDTO(blog);
        }

        public async Task<BlogResponseDTO> GetBlogBySlugAsync(string slug)
        {
            var blog = await _blogRepository.GetBySlugAsync(slug);
            if (blog != null)
            {
                blog.ViewCount++;
                await _blogRepository.UpdateAsync(blog);
            }
            return blog == null ? null : AutoMapper.ToBlogResponseDTO(blog);
        }

        public async Task<IEnumerable<BlogResponseDTO>> GetBlogsAsync(BlogQueryParamsDTO queryParams)
        {
            var blogs = await _blogRepository.GetAllAsync(queryParams);
            return AutoMapper.ToBlogResponseDTOs(blogs);
        }

        public async Task<int> GetBlogsCountAsync(BlogQueryParamsDTO queryParams)
        {
            return await _blogRepository.CountAsync(queryParams);
        }

        public async Task<BlogResponseDTO> UpdateBlogAsync(int blogId, UpdateBlogDTO updateBlogDto)
        {
            var existingBlog = await _blogRepository.GetByIdAsync(blogId);
            if (existingBlog == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy bài viết với ID {blogId}.");
            }

            if (existingBlog.Title != updateBlogDto.Title)
            {
                existingBlog.Slug = await GenerateUniqueSlug(updateBlogDto.Title);
            }

            if (updateBlogDto.IsPublished && !existingBlog.IsPublished)
            {
                existingBlog.PublishedAt = DateTime.UtcNow;
            }
            else if (!updateBlogDto.IsPublished)
            {
                existingBlog.PublishedAt = null;
            }

            AutoMapper.ApplyUpdatesToBlog(updateBlogDto, existingBlog);

            await _blogRepository.UpdateAsync(existingBlog);
            return AutoMapper.ToBlogResponseDTO(existingBlog);
        }

        public async Task<string> UpdateFeaturedImageAsync(int blogId, IFormFile imageFile)
        {
            var blog = await _blogRepository.GetByIdAsync(blogId);
            if (blog == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy bài viết với ID {blogId}.");
            }

            if (!string.IsNullOrEmpty(blog.FeaturedImage))
            {
                await _fileStorageService.DeleteFileAsync(blog.FeaturedImage);
            }

            var newImageUrl = await _fileStorageService.SaveFileAsync(imageFile, "blogs");
            blog.FeaturedImage = newImageUrl;
            await _blogRepository.UpdateAsync(blog);

            return newImageUrl;
        }

        private async Task<string> GenerateUniqueSlug(string title)
        {
            var baseSlug = GenerateSlug(title);
            var slug = baseSlug;
            int count = 1;
            while (await _blogRepository.GetBySlugAsync(slug) != null)
            {
                slug = $"{baseSlug}-{count++}";
            }
            return slug;
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
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }
    }
}
