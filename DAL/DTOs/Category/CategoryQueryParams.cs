using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.Category
{
    public class CategoryQueryParams
    {
        public string Search { get; set; }

        public bool? IsActive { get; set; }

        public int? ParentId { get; set; }

        public bool? HasParent { get; set; }

        public string SortBy { get; set; } = "name";

        public bool SortDescending { get; set; } = false;

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }

}
