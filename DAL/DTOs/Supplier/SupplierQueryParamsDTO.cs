
namespace DAL.DTOs.Supplier
{
    public class SupplierQueryParamsDTO
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; } // "name", "code", "createdAt"
        public string? SortOrder { get; set; } = "asc"; // "asc" or "desc"
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
