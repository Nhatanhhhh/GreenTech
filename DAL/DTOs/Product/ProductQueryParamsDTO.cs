

namespace DAL.DTOs.Product
{
    public class ProductQueryParamsDTO
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int? SupplierId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public string? SortBy { get; set; } // e.g., "name", "price", "createdAt"
        public string? SortOrder { get; set; } = "asc"; // "asc" or "desc"
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
