namespace DAL.DTOs.Review
{
    public class ReviewPaginationDTO
    {
        public List<ReviewResponseDTO> Reviews { get; set; } = new List<ReviewResponseDTO>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
