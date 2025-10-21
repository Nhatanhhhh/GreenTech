using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.Banner
{
    public class BannerQueryParams
    {
        public string Search { get; set; }

        public bool? IsActive { get; set; }

        public string Position { get; set; }

        public DateTime? StartDateFrom { get; set; }

        public DateTime? StartDateTo { get; set; }

        public string SortBy { get; set; } = "sortorder";

        public bool SortDescending { get; set; } = false;

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }
}
