namespace DAL.DTOs.User
{
    public class UserQueryParamsDTO
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; } // "ACTIVE", "BLOCKED", or null for all
        public string? SortBy { get; set; } // "fullName", "email", "createdAt", "points", "walletBalance"
        public string? SortOrder { get; set; } = "asc"; // "asc" or "desc"
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
