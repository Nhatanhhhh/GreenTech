
namespace DAL.DTOs.Blog
{
    public class BlogQueryParamsDTO
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int? AuthorId { get; set; }
        public bool? IsPublished { get; set; }
        public bool? IsFeatured { get; set; }
        public string? Tag { get; set; }
        public string? SortBy { get; set; } // "title", "createdAt", "publishedAt", "viewCount"
        public string? SortOrder { get; set; } = "desc"; // "asc" or "desc"
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
