
namespace DAL.DTOs.Blog
{
    public class BlogResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Excerpt { get; set; }
        public string Content { get; set; }
        public string FeaturedImage { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Tags { get; set; }
        public int ViewCount { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
